using System.Reflection;
using Microsoft.OpenApi.Models;
using ProductosApi.Data;
using ProductosApi.Middleware;
using ProductosApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. REGISTRO DE SERVICIOS (inyección de dependencias)
// ==========================================================

// Conexión: una sola fábrica para toda la app (solo guarda la cadena de conexión).
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

// Repositorio: una instancia por petición HTTP, expuesta por ambas interfaces.
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<IProductoRepository>(sp => sp.GetRequiredService<ProductoRepository>());
builder.Services.AddScoped<IProductoBusquedaRepository>(sp => sp.GetRequiredService<ProductoRepository>());

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Swagger: documentación y pantalla de pruebas.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Productos API",
        Version = "v1",
        Description = "CRUD de productos con ASP.NET Core + MySQL (ADO.NET / MySqlConnector)."
    });

    var archivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(archivoXml))
    {
        opciones.IncludeXmlComments(archivoXml);
    }
});

var app = builder.Build();

// ==========================================================
// 2. PIPELINE HTTP (orden en que pasa cada petición)
// ==========================================================

// Convierte errores de MySQL / no controlados en respuestas JSON (ProblemDetails).
app.UseMiddleware<DatabaseExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Productos API v1");
    opciones.DocumentTitle = "Productos API";
});

// La raíz redirige a Swagger para probar más rápido.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
