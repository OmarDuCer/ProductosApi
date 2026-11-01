using MySqlConnector;

namespace ProductosApi.Data;

/// <summary>
/// PUNTO 1 – CONEXIÓN.
/// Lee la cadena de conexión de appsettings.json y entrega conexiones MySQL abiertas.
/// MySqlConnector maneja un pool interno, así que abrir/cerrar por operación es barato.
/// </summary>
public class MySqlConnectionFactory : IDbConnectionFactory
{
    // ==========================================================
    // CONFIGURATION
    // ==========================================================
    public const string CONNECTION_STRING_NAME = "MySql";

    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(CONNECTION_STRING_NAME);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No se encontró la cadena de conexión '{CONNECTION_STRING_NAME}' en appsettings.json.");
        }

        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public MySqlConnection CrearConexion()
    {
        var conexion = new MySqlConnection(_connectionString);
        conexion.Open();
        return conexion;
    }
}
