using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PedidoClientManagement.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) EF + PostgreSQL
// -> instale antes: dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) CORS
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// 3) Controllers + JSON cycles off
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.WriteIndented = true;
    });

// 4) arquivos estáticos
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// **Auto-aplica migrações ao iniciar**
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
