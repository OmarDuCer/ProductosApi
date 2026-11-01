using MySqlConnector;

namespace ProductosApi.Data;

/// <summary>
/// Contrato para obtener conexiones a la base de datos.
/// El repositorio depende de esta interfaz y no de la cadena de conexión directamente.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Crea y ABRE una nueva conexión. Quien la pide es responsable de cerrarla (using).</summary>
    MySqlConnection CrearConexion();
}
