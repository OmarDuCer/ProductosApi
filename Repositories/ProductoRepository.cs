using MySqlConnector;
using ProductosApi.Data;
using ProductosApi.Models;

namespace ProductosApi.Repositories;

/// <summary>
/// Implementación de <see cref="IProductoRepository"/> con ADO.NET puro (MySqlConnector).
/// Aquí viven TODAS las sentencias SQL: consultas, insert, update, delete y búsqueda.
/// Todas las consultas son parametrizadas (@parametro) para evitar inyección SQL.
/// </summary>
public class ProductoRepository : IProductoRepository, IProductoBusquedaRepository
{
    // ==========================================================
    // CONFIGURATION
    // ==========================================================
    private const string TABLE_NAME = "productos";

    private const string COL_ID = "id";
    private const string COL_NOMBRE = "nombre";
    private const string COL_DESCRIPCION = "descripcion";
    private const string COL_CATEGORIA = "categoria";
    private const string COL_PRECIO = "precio";
    private const string COL_STOCK = "stock";
    private const string COL_ACTIVO = "activo";
    private const string COL_FECHA_REGISTRO = "fecha_registro";

    private const string SELECT_COLUMNS =
        $"{COL_ID}, {COL_NOMBRE}, {COL_DESCRIPCION}, {COL_CATEGORIA}, " +
        $"{COL_PRECIO}, {COL_STOCK}, {COL_ACTIVO}, {COL_FECHA_REGISTRO}";

    private const int NO_ROWS_AFFECTED = 0;
    private const int INVALID_ID = 0;

    // ==========================================================
    // SQL STATEMENTS
    // ==========================================================
    private const string SQL_SELECT_ALL =
        $"SELECT {SELECT_COLUMNS} FROM {TABLE_NAME} ORDER BY {COL_ID};";

    private const string SQL_SELECT_BY_ID =
        $"SELECT {SELECT_COLUMNS} FROM {TABLE_NAME} WHERE {COL_ID} = @id;";

    private const string SQL_INSERT =
        $"INSERT INTO {TABLE_NAME} ({COL_NOMBRE}, {COL_DESCRIPCION}, {COL_CATEGORIA}, {COL_PRECIO}, {COL_STOCK}, {COL_ACTIVO}) " +
        "VALUES (@nombre, @descripcion, @categoria, @precio, @stock, @activo);";

    private const string SQL_UPDATE =
        $"UPDATE {TABLE_NAME} SET {COL_NOMBRE} = @nombre, {COL_DESCRIPCION} = @descripcion, " +
        $"{COL_CATEGORIA} = @categoria, {COL_PRECIO} = @precio, {COL_STOCK} = @stock, {COL_ACTIVO} = @activo " +
        $"WHERE {COL_ID} = @id;";

    private const string SQL_DELETE =
        $"DELETE FROM {TABLE_NAME} WHERE {COL_ID} = @id;";

    private const string SQL_SEARCH =
        $"SELECT {SELECT_COLUMNS} FROM {TABLE_NAME} " +
        $"WHERE {COL_NOMBRE} LIKE @patron OR {COL_DESCRIPCION} LIKE @patron OR {COL_CATEGORIA} LIKE @patron " +
        $"ORDER BY {COL_NOMBRE};";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ProductoRepository> _logger;

    public ProductoRepository(IDbConnectionFactory connectionFactory, ILogger<ProductoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // ==========================================================
    // PUNTO 2 – CONSULTAS
    // ==========================================================

    /// <summary>Devuelve todos los productos ordenados por id.</summary>
    public List<Producto> ObtenerTodos()
    {
        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_SELECT_ALL, conexion);

        return LeerProductos(comando);
    }

    /// <summary>Devuelve un producto por su id, o null si no existe.</summary>
    public Producto? ObtenerPorId(int id)
    {
        if (id <= INVALID_ID)
        {
            return null;
        }

        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_SELECT_BY_ID, conexion);
        comando.Parameters.AddWithValue("@id", id);

        return LeerProductos(comando).FirstOrDefault();
    }

    // ==========================================================
    // PUNTO 3 – INSERT
    // ==========================================================

    /// <summary>Inserta un producto y devuelve el id generado por MySQL.</summary>
    public int Insertar(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_INSERT, conexion);
        AgregarParametrosDeDatos(comando, producto);

        comando.ExecuteNonQuery();

        var nuevoId = Convert.ToInt32(comando.LastInsertedId);
        producto.Id = nuevoId;

        _logger.LogInformation("Producto insertado con id {Id}", nuevoId);
        return nuevoId;
    }

    // ==========================================================
    // PUNTO 4 – ACTUALIZAR
    // ==========================================================

    /// <summary>Actualiza un producto existente. Devuelve false si el id no existe.</summary>
    public bool Actualizar(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        if (producto.Id <= INVALID_ID)
        {
            return false;
        }

        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_UPDATE, conexion);
        AgregarParametrosDeDatos(comando, producto);
        comando.Parameters.AddWithValue("@id", producto.Id);

        // MySqlConnector regresa las filas ENCONTRADAS por el WHERE (UseAffectedRows=false por defecto):
        // si el id no existe regresa 0; si existe regresa 1 aunque los datos no hayan cambiado.
        var filasAfectadas = comando.ExecuteNonQuery();
        return filasAfectadas > NO_ROWS_AFFECTED;
    }

    // ==========================================================
    // PUNTO 5 – ELIMINAR
    // ==========================================================

    /// <summary>Elimina un producto por id. Devuelve false si no existía.</summary>
    public bool Eliminar(int id)
    {
        if (id <= INVALID_ID)
        {
            return false;
        }

        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_DELETE, conexion);
        comando.Parameters.AddWithValue("@id", id);

        var filasAfectadas = comando.ExecuteNonQuery();
        if (filasAfectadas > NO_ROWS_AFFECTED)
        {
            _logger.LogInformation("Producto {Id} eliminado", id);
        }

        return filasAfectadas > NO_ROWS_AFFECTED;
    }

    // ==========================================================
    // PUNTO 6 – BUSCAR
    // ==========================================================

    /// <summary>Busca productos cuyo nombre, descripción o categoría contengan el término.</summary>
    public List<Producto> Buscar(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return new List<Producto>();
        }

        using var conexion = _connectionFactory.CrearConexion();
        using var comando = new MySqlCommand(SQL_SEARCH, conexion);
        comando.Parameters.AddWithValue("@patron", $"%{EscaparPatronLike(termino.Trim())}%");

        return LeerProductos(comando);
    }

    // ==========================================================
    // HELPERS
    // ==========================================================

    /// <summary>Ejecuta el comando y convierte cada fila válida en un Producto. Las filas corruptas se saltan.</summary>
    private List<Producto> LeerProductos(MySqlCommand comando)
    {
        var productos = new List<Producto>();

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var producto = MapearProducto(lector);
            if (producto is not null)
            {
                productos.Add(producto);
            }
        }

        return productos;
    }

    /// <summary>Convierte la fila actual del lector en un Producto, o null si la fila no es válida.</summary>
    private Producto? MapearProducto(MySqlDataReader lector)
    {
        try
        {
            return new Producto
            {
                Id = lector.GetInt32(lector.GetOrdinal(COL_ID)),
                Nombre = LeerTexto(lector, COL_NOMBRE) ?? string.Empty,
                Descripcion = LeerTexto(lector, COL_DESCRIPCION),
                Categoria = LeerTexto(lector, COL_CATEGORIA) ?? string.Empty,
                Precio = lector.GetDecimal(lector.GetOrdinal(COL_PRECIO)),
                Stock = lector.GetInt32(lector.GetOrdinal(COL_STOCK)),
                Activo = lector.GetBoolean(lector.GetOrdinal(COL_ACTIVO)),
                FechaRegistro = lector.GetDateTime(lector.GetOrdinal(COL_FECHA_REGISTRO))
            };
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or IndexOutOfRangeException)
        {
            _logger.LogWarning(ex, "Se omitió una fila de '{Tabla}' con datos inválidos", TABLE_NAME);
            return null;
        }
    }

    /// <summary>Lee una columna de texto que puede ser NULL.</summary>
    private static string? LeerTexto(MySqlDataReader lector, string columna)
    {
        var ordinal = lector.GetOrdinal(columna);
        return lector.IsDBNull(ordinal) ? null : lector.GetString(ordinal);
    }

    /// <summary>Agrega los parámetros comunes de INSERT y UPDATE.</summary>
    private static void AgregarParametrosDeDatos(MySqlCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        comando.Parameters.AddWithValue("@categoria", producto.Categoria);
        comando.Parameters.AddWithValue("@precio", producto.Precio);
        comando.Parameters.AddWithValue("@stock", producto.Stock);
        comando.Parameters.AddWithValue("@activo", producto.Activo);
    }

    /// <summary>Escapa los comodines de LIKE (% y _) para que se busquen como texto literal.</summary>
    private static string EscaparPatronLike(string texto) =>
        texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
