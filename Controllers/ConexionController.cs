using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProductosApi.Data;

namespace ProductosApi.Controllers;

/// <summary>
/// Endpoint de diagnóstico para comprobar que la API puede conectarse a MySQL.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConexionController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConexionController> _logger;

    public ConexionController(IDbConnectionFactory connectionFactory, ILogger<ConexionController> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Abre una conexión de prueba y devuelve la versión del servidor y la base de datos.</summary>
    /// <response code="200">Conexión exitosa.</response>
    /// <response code="503">No fue posible conectarse a MySQL.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Probar()
    {
        try
        {
            using var conexion = _connectionFactory.CrearConexion();
            return Ok(new
            {
                conectado = true,
                servidor = conexion.ServerVersion,
                baseDeDatos = conexion.Database
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Falló la prueba de conexión a MySQL");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Sin conexión a la base de datos",
                detail: ex.Message);
        }
    }
}
