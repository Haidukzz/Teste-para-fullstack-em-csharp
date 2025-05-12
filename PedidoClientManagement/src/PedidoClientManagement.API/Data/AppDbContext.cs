using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PedidoClientManagement.API.Models;

namespace PedidoClientManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Tabelas (DbSets)
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índice único para CPF (cliente)
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CPF)
                .IsUnique();

            // Configuração global de precisão para propriedades decimais
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var decimalProperties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

                foreach (var property in decimalProperties)
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
    }
}