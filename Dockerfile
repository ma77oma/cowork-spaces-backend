FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/CoworkSpaces.Api/CoworkSpaces.Api.csproj", "src/CoworkSpaces.Api/"]
COPY ["src/CoworkSpaces.Application/CoworkSpaces.Application.csproj", "src/CoworkSpaces.Application/"]
COPY ["src/CoworkSpaces.Domain/CoworkSpaces.Domain.csproj", "src/CoworkSpaces.Domain/"]
COPY ["src/CoworkSpaces.Infrastructure/CoworkSpaces.Infrastructure.csproj", "src/CoworkSpaces.Infrastructure/"]

RUN dotnet restore "src/CoworkSpaces.Api/CoworkSpaces.Api.csproj"

COPY . .

RUN dotnet publish "src/CoworkSpaces.Api/CoworkSpaces.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CoworkSpaces.Api.dll"]
