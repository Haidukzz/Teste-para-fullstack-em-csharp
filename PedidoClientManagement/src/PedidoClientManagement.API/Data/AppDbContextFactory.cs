using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PedidoClientManagement.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseNpgsql(
                "Host=dpg-d0gm3dadbo4c73bh5hqg-a.oregon-postgres.render.com;" +
                "Port=5432;" +
                "Database=pedido_client_db;" +
                "Username=haidukzz;" +
                "Password=Futebol0908@;" +
                "SSL Mode=Require;Trust Server Certificate=true"
            );
            return new AppDbContext(builder.Options);
        }
    }
}
