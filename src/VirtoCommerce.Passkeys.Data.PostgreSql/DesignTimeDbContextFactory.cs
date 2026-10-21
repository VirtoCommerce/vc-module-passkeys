using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.Passkeys.Data.Repositories;

namespace VirtoCommerce.Passkeys.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PasskeysDbContext>
{
    public PasskeysDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PasskeysDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(PostgreSqlDataAssemblyMarker).Assembly.GetName().Name));

        return new PasskeysDbContext(builder.Options);
    }
}
