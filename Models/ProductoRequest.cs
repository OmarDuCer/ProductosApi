namespace ProductosApi.Models;

/// <summary>
/// Datos que el cliente envía para crear o actualizar un producto.
/// No incluye <c>Id</c> ni <c>FechaRegistro</c> porque esos los controla el servidor.
/// </summary>
public class ProductoRequest
{
    /// <example>Teclado mecánico</example>
    public string? Nombre { get; set; }

    /// <example>Teclado con switches rojos y retroiluminación RGB</example>
    public string? Descripcion { get; set; }

    /// <example>Electrónica</example>
    public string? Categoria { get; set; }

    /// <example>899.50</example>
    public decimal Precio { get; set; }

    /// <example>25</example>
    public int Stock { get; set; }

    /// <example>true</example>
    public bool Activo { get; set; } = true;

    /// <summary>Convierte la petición en una entidad, limpiando espacios.</summary>
    public Producto ToProducto(int id = 0) => new()
    {
        Id = id,
        Nombre = Nombre?.Trim() ?? string.Empty,
        Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
        Categoria = Categoria?.Trim() ?? string.Empty,
        Precio = Precio,
        Stock = Stock,
        Activo = Activo
    };
}
