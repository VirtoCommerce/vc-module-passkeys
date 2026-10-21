using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.Passkeys.Data.Repositories;

namespace VirtoCommerce.Passkeys.Data.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PasskeysDbContext>
{
    public PasskeysDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PasskeysDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=(local);User=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseSqlServer(
            connectionString,
            options => options.MigrationsAssembly(typeof(SqlServerDataAssemblyMarker).Assembly.GetName().Name));

        return new PasskeysDbContext(builder.Options);
    }
}
