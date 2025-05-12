using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PedidoClientManagement.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) String fixa com sua senha embutida
var fixedConnectionString =
    "Host=dpg-d0h4j33e5dus73d4ifcg-a;" +  // << host interno, sem .render.com
    "Port=5432;" +
    "Database=pedido_client_db_1164;" +
    "Username=haidukzz;" +
    "Password=6rs2dR2WYslMVvJwi7uwRPnes6U8BDFg;" +
    "SSL Mode=Require;Trust Server Certificate=true";

Console.WriteLine($"→ Usando internal connection string: {fixedConnectionString}");

// 2) EF + PostgreSQL usando a string fixa
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(fixedConnectionString));

// 3) CORS
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// 4) Controllers + JSON cycles off
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.WriteIndented = true;
    });

// 5) Arquivos estáticos
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// **Auto-aplica migrações no startup**
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseCors();

// default files -> wwwroot/index.html
var df = new DefaultFilesOptions();
df.DefaultFileNames.Clear();
df.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(df);

app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
