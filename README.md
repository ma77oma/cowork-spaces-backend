# CoworkSpaces

Backend en .NET 8 para la prueba tecnica "Sistema de Gestion de Reservas con Control de Concurrencia y Tarifas Dinamicas".

## Stack

- .NET 8 Web API
- C#
- Entity Framework Core 8
- SQL Server
- Swagger / OpenAPI
- xUnit
- Clean Architecture simple
- MediatR
- FluentValidation
- ASP.NET Core Identity
- JWT Bearer

## Estructura

```text
CoworkSpaces/
├── src/
│   ├── CoworkSpaces.Api
│   ├── CoworkSpaces.Application
│   ├── CoworkSpaces.Domain
│   └── CoworkSpaces.Infrastructure
├── tests/
│   └── CoworkSpaces.Tests
├── scripts/
│   └── concurrency-test.js
├── requests/
│   └── cowork-spaces.http
└── README.md
```

## Arquitectura

- `CoworkSpaces.Domain`: entidades, enums y servicios de dominio puros.
- `CoworkSpaces.Application`: DTOs, features CQRS con MediatR, validaciones de negocio y excepciones.
- `CoworkSpaces.Infrastructure`: `AppDbContext`, Identity, configuraciones Fluent API, SQL Server, migraciones y seed.
- `CoworkSpaces.Api`: controllers REST, Swagger, autenticacion JWT, middleware global de errores y composicion de dependencias.

## Decisiones de arquitectura y trade-offs

Decisiones principales:

- se uso Clean Architecture simple para separar dominio, casos de uso, infraestructura y API sin sobreingenierizar;
- se uso CQRS simple con MediatR para mantener controllers delgados y aislar cada caso de uso en handlers pequeños;
- se mantuvieron `PricingService` y `CancellationPolicyService` en dominio para que la logica critica sea pura y testeable;
- se eligio Entity Framework Core con SQL Server para productividad, migraciones y soporte transaccional nativo;
- se agrego ASP.NET Core Identity con JWT para asociar reservas al usuario autenticado y proteger endpoints.

Trade-offs asumidos:

- no se uso una arquitectura CQRS compleja con buses, proyecciones ni almacenamiento separado de lectura/escritura, porque el alcance de la prueba no lo requiere;
- no se uso un motor de cache ni optimizaciones avanzadas de lectura, privilegiando claridad y mantenibilidad;
- se eligio una solucion de concurrencia pesimista, que es mas fuerte para evitar dobles reservas, a costa de mayor contencion sobre el espacio durante la transaccion;
- el job de completado corre dentro de la propia API con Quartz, lo que simplifica despliegue, aunque en un sistema de mayor escala podria externalizarse a un worker dedicado;
- los reportes se calculan en tiempo real desde la base transaccional, lo que simplifica consistencia, aunque para volumenes altos podria convenir materializar agregados.

## Uso de CQRS y MediatR

Se usa CQRS simple con MediatR para separar lecturas y escrituras sin sobreingenierizar la solucion.

- los `Commands` modifican estado, por ejemplo crear, actualizar o cancelar;
- las `Queries` solo consultan datos y no alteran el estado del sistema;
- los controllers quedan delgados y delegan los casos de uso a `IMediator`;
- la validacion transversal se ejecuta con `FluentValidation` mediante `ValidationBehavior`.

La concurrencia no depende de CQRS. La resolucion de conflictos sigue implementandose dentro de `CreateReservationCommandHandler` con transaccion `Serializable` y bloqueo pesimista `UPDLOCK/HOLDLOCK`.

Se eligio CQRS simple con MediatR para separar claramente los casos de uso de escritura y lectura, mantener controllers delgados, facilitar pruebas unitarias y evitar servicios de aplicacion demasiado grandes. La estrategia de concurrencia sigue implementandose a nivel de base de datos usando transaccion Serializable y bloqueo pesimista con UPDLOCK/HOLDLOCK.

## Autenticacion

La API integra `ASP.NET Core Identity` y `JWT Bearer`.

- `POST /api/auth/register`: registra usuario y devuelve token.
- `POST /api/auth/login`: autentica usuario y devuelve token.
- las reservas quedan asociadas al usuario autenticado mediante `CreatedByUserId`.
- cancelar y consultar una reserva requiere ser el propietario o un usuario con rol `Admin`.

Usuario admin seed inicial:

- email: `admin@coworkspaces.local`
- password: `Admin123`

La cuenta admin no debe crearse manualmente. Se genera automaticamente durante el seed inicial si no existe en la base de datos.

## Job de completado de reservas

La aplicacion ejecuta un job de Quartz cada 5 minutos para marcar como `Completed` las reservas que:

- esten en estado `Confirmed`;
- tengan `EndAt <= DateTime.Now`.

Implementacion:

- job: `src/CoworkSpaces.Infrastructure/Jobs/CompleteReservationsJob.cs`
- scheduler: registrado en `src/CoworkSpaces.Infrastructure/DependencyInjection.cs`

Cron configurado:

```text
0 0/5 * * * ?
```

Esto significa que el proceso corre cada 5 minutos mientras la API este levantada.

## Reglas del reporte

- la tasa de ocupacion por espacio se calcula sobre el total de horas disponibles del espacio dentro del rango consultado, usando su horario de apertura y cierre;
- la ocupacion y los ingresos del reporte consideran solo reservas en estado `Confirmed` o `Completed`;
- la hora mas demandada se agrupa por hora de inicio exacta de la reserva, por ejemplo `09:00`, `10:00`, `17:00`.

## Reglas principales

### Tarifas dinamicas

Orden elegido de aplicacion:

1. Precio base = tarifa base por hora x duracion en horas.
2. Hora pico 09:00-11:00 o 17:00-19:00: +25%.
3. Fin de semana: +15%.
4. Reserva larga de 4 horas o mas: -10%.
5. Anticipacion de 7 dias o mas: -5%.

Decisiones documentadas para las reglas de tarifa:

- si una reserva toca parcialmente una franja pico, el +25% se aplica a toda la reserva;
- las reglas se aplican de forma acumulativa en el orden listado arriba;
- el redondeo a 2 decimales se realiza solo al final del calculo completo;
- la regla de anticipacion de 7 dias usa fecha y hora exacta, no solo fecha calendario.

### Politica de cancelacion

- Mas de 48 horas: 100% de reembolso.
- Entre 24 y 48 horas, incluyendo exactamente 48 y 24: 50%.
- Menos de 24 horas: 0%.
- Una reserva completada no se puede cancelar.

Para disponibilidad, bloquean horario las reservas en estado `Pending` y `Confirmed`.

### Concurrencia transaccional

La creacion de reservas usa una transaccion SQL Server con `IsolationLevel.Serializable` y bloqueo pesimista sobre la fila del espacio:

```sql
SELECT * FROM Spaces WITH (UPDLOCK, HOLDLOCK) WHERE Id = @spaceId
```

Luego se valida el solapamiento dentro de la misma transaccion usando:

```text
existing.StartAt < newEnd && newStart < existing.EndAt
```

Con esta estrategia, dos peticiones simultaneas al mismo espacio y horario no pueden confirmar ambas reservas. Una crea la reserva y la otra recibe `409 Conflict`.

La validacion de disponibilidad considera solapamiento solo contra reservas `Pending` y `Confirmed`. Las reservas `Cancelled` no bloquean disponibilidad, y las `Completed` no afectan nuevos horarios futuros.

### Estrategia de concurrencia elegida

La estrategia implementada es `pessimistic locking`.

En SQL Server no se usa literalmente `SELECT ... FOR UPDATE`, sino su equivalente mediante hints de bloqueo:

```sql
SELECT * FROM Spaces WITH (UPDLOCK, HOLDLOCK) WHERE Id = @spaceId
```

Esta consulta se ejecuta dentro de una transaccion con aislamiento `Serializable`, lo que garantiza que:

- una solicitud bloquee el recurso del espacio mientras valida disponibilidad;
- ninguna otra solicitud concurrente pueda confirmar la misma franja horaria antes de que termine la transaccion;
- si existe conflicto de solapamiento, la segunda solicitud reciba `409 Conflict`.

No se utilizo `optimistic concurrency` porque el problema principal no era resolver actualizaciones concurrentes sobre una misma fila editada por varios usuarios, sino evitar dobles reservas simultaneas en una ventana de tiempo. Tampoco se utilizo `exclusion constraint`, ya que SQL Server no ofrece este mecanismo de forma nativa como si ocurre en PostgreSQL.

## Indices

Se crean los indices:

- `IX_Reservations_SpaceId_StartAt_EndAt_Status`
- `IX_Reservations_StartAt_EndAt`

Justificacion:

- aceleran la validacion de disponibilidad por espacio;
- reducen el costo de la busqueda de solapamientos por rango temporal;
- mejoran consultas de reportes por fechas y agregaciones sobre reservas activas.

## Seed inicial

Espacios iniciales:

- `11111111-1111-1111-1111-111111111111` - Sala Ejecutiva
- `22222222-2222-2222-2222-222222222222` - Sala Directorio
- `33333333-3333-3333-3333-333333333333` - Sala Creativa

## Cadena de conexion

Antes de ejecutar el proyecto, actualiza la cadena de conexion con una instancia real de SQL Server disponible en tu equipo o entorno.

Archivos a revisar:

- `src/CoworkSpaces.Api/appsettings.json`
- `src/CoworkSpaces.Api/appsettings.Development.json`
- `src/CoworkSpaces.Infrastructure/Persistence/AppDbContextFactory.cs`

Ejemplo de formato:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR\\SQLEXPRESS;Database=CoworkSpacesDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Si descargas este repositorio, no asumas que la instancia incluida en el ejemplo existe en tu maquina. Debes reemplazar `TU_SERVIDOR\\SQLEXPRESS` por tu servidor real.

## Comandos

### Restaurar paquetes

```bash
dotnet restore CoworkSpaces.sln
```

### Crear base de datos y ejecutar migraciones

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/CoworkSpaces.Infrastructure --startup-project src/CoworkSpaces.Api
```

Tambien puedes simplemente levantar la API:

```bash
dotnet run --project src/CoworkSpaces.Api
```

La aplicacion ejecuta `Database.MigrateAsync()` al iniciar, por lo que aplica automaticamente las migraciones pendientes antes de atender solicitudes. Despues de eso, se ejecuta el seed de roles, usuario admin y datos iniciales.

### Levantar con Docker Compose

El repositorio incluye `Dockerfile` y un `docker-compose.yml` funcional para levantar la API junto con SQL Server.

1. Crea un archivo `.env` en la raiz del proyecto. Puedes copiar `.env.example` o usar este contenido base:

```env
MSSQL_SA_PASSWORD=YourStrong!Passw0rd
SQLSERVER_DB=CoworkSpacesDb
API_PORT=8080
SQLSERVER_PORT=1433
```

2. Levanta los servicios:

```bash
docker compose up --build
```

3. Abre Swagger en:

- `http://localhost:8080/swagger`

4. Si cambias `API_PORT` o `SQLSERVER_PORT` en `.env`, los puertos expuestos pasan a ser:

- API: `http://localhost:${API_PORT}/swagger`
- SQL Server: `localhost,${SQLSERVER_PORT}`

5. Para detener los servicios:

```bash
docker compose down
```

Detalles:

- la API usa la cadena de conexion inyectada por `docker-compose.yml` mediante `ConnectionStrings__DefaultConnection`;
- al iniciar, la API aplica migraciones automaticamente y luego ejecuta el seed inicial;
- la redireccion HTTPS se omite dentro del contenedor para evitar redirecciones a un puerto TLS no publicado;
- `docker compose` toma los valores desde el archivo `.env`.

### Crear una nueva migracion

```bash
dotnet ef migrations add AddIdentityAndReservationUsers --project src/CoworkSpaces.Infrastructure --startup-project src/CoworkSpaces.Api --output-dir Persistence/Migrations
```

### Levantar API

```bash
dotnet run --project src/CoworkSpaces.Api
```

### Ejecutar tests

```bash
dotnet test CoworkSpaces.sln
```

### Ejecutar script de concurrencia

```bash
node scripts/concurrency-test.js
```

### Ejecutar test de integracion de concurrencia

```bash
dotnet test CoworkSpaces.sln --filter "FullyQualifiedName~ReservationConcurrencyTests"
```

Variables opcionales del script:

```bash
API_BASE_URL=http://localhost:5254
SPACE_ID=11111111-1111-1111-1111-111111111111
TEST_EMAIL=usuario.prueba@coworkspaces.local
TEST_PASSWORD=Test1234
```

Si quieres ejecutar la prueba con otro usuario, define `TEST_EMAIL` y `TEST_PASSWORD` antes de correr el script.

Ejemplo en PowerShell:

```powershell
$env:NODE_TLS_REJECT_UNAUTHORIZED="0"
$env:API_BASE_URL="https://localhost:7081"
$env:TEST_EMAIL="otro.usuario@coworkspaces.local"
$env:TEST_PASSWORD="Test1234"
node scripts/concurrency-test.js
```

El script intenta primero registrar ese usuario. Si ya existe, hace login con esas mismas credenciales y reutiliza el token para las dos solicitudes concurrentes.

## Endpoints principales

- `GET /api/spaces`
- `GET /api/spaces/{id}`
- `POST /api/spaces`
- `PUT /api/spaces/{id}`
- `DELETE /api/spaces/{id}`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/reservations`
- `POST /api/reservations/preview-price`
- `GET /api/reservations/my`
- `POST /api/reservations/{id}/cancel`
- `GET /api/reservations/{id}`
- `GET /api/reports?from=2026-06-01&to=2026-06-30`

### Respuesta ampliada del endpoint de reportes

El endpoint `GET /api/reports` mantiene los campos originales y ahora ademas devuelve agregados listos para graficas en frontend:

- `totalReservations`: total de reservas consideradas en el rango;
- `totalSpacesAnalyzed`: cantidad de espacios analizados;
- `incomeByDay`: serie diaria de ingresos;
- `reservationsByDay`: serie diaria de reservas;
- `demandByHour`: cantidad de reservas agrupadas por hora exacta de inicio;
- `incomeBySpace`: ingresos totales por espacio listos para grafica de barras o pie;
- `occupancyBySpace`: ocupacion por espacio lista para grafica comparativa.

Ejemplo resumido:

```json
{
  "totalIncome": 200,
  "totalReservations": 1,
  "totalSpacesAnalyzed": 3,
  "mostDemandedHour": "10:00",
  "spaces": [
    {
      "spaceId": "11111111-1111-1111-1111-111111111111",
      "spaceName": "Sala Ejecutiva",
      "occupancyRate": 3.33,
      "totalIncome": 200,
      "totalReservations": 1
    }
  ],
  "incomeByDay": [
    {
      "date": "2026-06-16",
      "totalIncome": 200
    }
  ],
  "reservationsByDay": [
    {
      "date": "2026-06-16",
      "totalReservations": 1
    }
  ],
  "demandByHour": [
    {
      "hour": "10:00",
      "totalReservations": 1
    }
  ],
  "incomeBySpace": [
    {
      "spaceId": "11111111-1111-1111-1111-111111111111",
      "spaceName": "Sala Ejecutiva",
      "totalIncome": 200
    }
  ],
  "occupancyBySpace": [
    {
      "spaceId": "11111111-1111-1111-1111-111111111111",
      "spaceName": "Sala Ejecutiva",
      "occupancyRate": 3.33
    }
  ]
}
```

## Manejo global de errores

El middleware devuelve respuestas en formato estandar:

```json
{
  "statusCode": 409,
  "message": "El espacio ya esta reservado en ese horario.",
  "details": null
}
```

## Supuestos

- las reservas deben iniciar y terminar el mismo dia;
- el ingreso reportado es neto: `FinalPrice - RefundAmount`;
- las reservas canceladas no bloquean disponibilidad ni se cuentan en reportes de ocupacion/demanda;
- se usa hora local del servidor para validaciones de negocio y cancelaciones;
- los endpoints de reserva requieren autenticacion JWT.

## Prueba de concurrencia

La concurrencia fue probada de dos maneras:

1. `scripts/concurrency-test.js`
: envía dos solicitudes paralelas a `POST /api/reservations` usando el mismo espacio y rango horario. El script autentica un usuario, lanza ambas peticiones en paralelo y valida visualmente que una responda `201` y la otra `409`.

2. `ReservationConcurrencyTests`
: test de integracion en .NET con `WebApplicationFactory`, `HttpClient` y SQL Server real. El test crea una base aislada para la corrida, registra un usuario, ejecuta dos `POST /api/reservations` concurrentes y verifica que:
- una respuesta sea `201 Created`;
- la otra sea `409 Conflict`;
- solo exista una reserva confirmada en base de datos para ese espacio y horario.

### Como ejecutar este caso

#### Opcion 1: test de integracion en .NET

Ejecuta desde la raiz del repositorio:

```bash
dotnet test CoworkSpaces.sln --filter "FullyQualifiedName~ReservationConcurrencyTests"
```

Resultado esperado:

- una solicitud devuelve `201 Created`;
- la otra devuelve `409 Conflict`;
- nunca quedan dos reservas confirmadas para el mismo espacio y horario.

Si deseas apuntar a otra instancia de SQL Server para esta prueba:

```bash
TEST_SQLSERVER_CONNECTION="Server=TU_SERVIDOR\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet test CoworkSpaces.sln --filter "FullyQualifiedName~ReservationConcurrencyTests"
```

En PowerShell:

```powershell
$env:TEST_SQLSERVER_CONNECTION="Server=TU_SERVIDOR\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet test CoworkSpaces.sln --filter "FullyQualifiedName~ReservationConcurrencyTests"
```

#### Opcion 2: script manual con requests paralelos

1. Levanta la API:

```bash
dotnet run --project src/CoworkSpaces.Api
```

2. En otra terminal ejecuta:

```bash
node scripts/concurrency-test.js
```

El script:

- registra o autentica un usuario automaticamente;
- envía dos requests paralelos a `POST /api/reservations`;
- usa el mismo `SpaceId`, `StartAt` y `EndAt` en ambas peticiones;
- imprime ambos codigos HTTP y sus respuestas.

Resultado esperado:

- una respuesta `201 Created`;
- una respuesta `409 Conflict`;
- nunca deben crearse ambas reservas.

### Evidencia esperada

#### Salida esperada del test de integracion

```text
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1
```

El test valida internamente que:

- una peticion obtuvo `201 Created`;
- la otra obtuvo `409 Conflict`;
- en base de datos solo existe una reserva confirmada para ese horario.

#### Salida esperada del script manual

Ejemplo de salida:

```text
Payload: {
  spaceId: '11111111-1111-1111-1111-111111111111',
  startAt: '2026-06-16T10:00:00.000Z',
  endAt: '2026-06-16T12:00:00.000Z'
}
Request 1: 201
{ ...respuesta de reserva creada... }
Request 2: 409
{"statusCode":409,"message":"El espacio ya esta reservado en ese horario.","details":null}
Expected: one 201 Created and one 409 Conflict.
```

El orden puede invertirse. Tambien es valido que `Request 1` sea `409` y `Request 2` sea `201`. Lo importante es que nunca sean `201` las dos.
