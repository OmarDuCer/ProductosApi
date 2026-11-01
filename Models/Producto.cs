namespace ProductosApi.Models;

/// <summary>
/// Entidad que representa un producto. Cada propiedad corresponde a una columna
/// de la tabla <c>productos</c> en MySQL.
/// </summary>
public class Producto
{
    /// <summary>Identificador único (AUTO_INCREMENT en la base de datos).</summary>
    public int Id { get; set; }

    /// <summary>Nombre comercial del producto.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción opcional.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Categoría a la que pertenece (ej. Electrónica, Papelería).</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Precio unitario en pesos mexicanos.</summary>
    public decimal Precio { get; set; }

    /// <summary>Unidades disponibles en inventario.</summary>
    public int Stock { get; set; }

    /// <summary>Indica si el producto está disponible para venta.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Fecha en que se registró (la asigna MySQL).</summary>
    public DateTime FechaRegistro { get; set; }
}
