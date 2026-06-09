using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoworkSpaces.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-0U4OQF0\\SQLEXPRESS;Database=CoworkSpacesDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        return new AppDbContext(optionsBuilder.Options);
    }
}
