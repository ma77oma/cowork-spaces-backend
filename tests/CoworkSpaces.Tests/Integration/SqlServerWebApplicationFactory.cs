using CoworkSpaces.Api;
using CoworkSpaces.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoworkSpaces.Tests.Integration;

public class SqlServerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"CoworkSpacesTest_{Guid.NewGuid():N}";
    private readonly string _baseConnectionString;

    public SqlServerWebApplicationFactory()
    {
        _baseConnectionString = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
            ?? "Server=DESKTOP-0U4OQF0\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
    }

    public string ConnectionString
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(_baseConnectionString)
            {
                InitialCatalog = _databaseName
            };

            return builder.ConnectionString;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Issuer"] = "CoworkSpaces.Api",
                ["Jwt:Audience"] = "CoworkSpaces.Client",
                ["Jwt:Key"] = "CoworkSpaces.SuperSecretKey.ForJwt.Token.2026",
                ["Jwt:ExpirationMinutes"] = "120"
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await IdentityDataSeeder.SeedAsync(Services);
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}
