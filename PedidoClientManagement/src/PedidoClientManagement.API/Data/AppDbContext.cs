using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata;
using PedidoClientManagement.API.Models;

namespace PedidoClientManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

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

            // Conversor global: sempre ler/gravar DateTime como UTC
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc
                     ? v
                     : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            // Aplicar converter a todas as propriedades DateTime do modelo
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // decimal precision
                var decimalProps = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));
                foreach (var prop in decimalProps)
                {
                    prop.SetPrecision(18);
                    prop.SetScale(2);
                }

                // DateTime UTC converter
                var dateTimeProps = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime));
                foreach (var prop in dateTimeProps)
                {
                    prop.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}
