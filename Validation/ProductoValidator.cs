using ProductosApi.Models;

namespace ProductosApi.Validation;

/// <summary>
/// Reglas de negocio que debe cumplir un producto antes de llegar a la base de datos.
/// Los límites coinciden con las longitudes de columna definidas en database/script.sql.
/// </summary>
public static class ProductoValidator
{
    // ==========================================================
    // CONFIGURATION
    // ==========================================================
    public const int NOMBRE_MAX_LENGTH = 100;
    public const int DESCRIPCION_MAX_LENGTH = 255;
    public const int CATEGORIA_MAX_LENGTH = 50;
    public const decimal PRECIO_MINIMO = 0m;
    public const decimal PRECIO_MAXIMO = 99_999_999.99m;
    public const int STOCK_MINIMO = 0;
    public const int TERMINO_BUSQUEDA_MIN_LENGTH = 2;

    /// <summary>
    /// Valida la petición y devuelve un diccionario campo → errores (formato que entiende ValidationProblem).
    /// Si el diccionario está vacío, la petición es válida.
    /// </summary>
    public static Dictionary<string, string[]> Validar(ProductoRequest? request)
    {
        var errores = new Dictionary<string, string[]>();

        if (request is null)
        {
            errores["body"] = new[] { "El cuerpo de la petición es obligatorio." };
            return errores;
        }

        ValidarTextoObligatorio(errores, nameof(request.Nombre), request.Nombre, NOMBRE_MAX_LENGTH);
        ValidarTextoObligatorio(errores, nameof(request.Categoria), request.Categoria, CATEGORIA_MAX_LENGTH);

        if (request.Descripcion is not null && request.Descripcion.Trim().Length > DESCRIPCION_MAX_LENGTH)
        {
            errores[nameof(request.Descripcion)] =
                new[] { $"La descripción no puede exceder {DESCRIPCION_MAX_LENGTH} caracteres." };
        }

        if (request.Precio < PRECIO_MINIMO || request.Precio > PRECIO_MAXIMO)
        {
            errores[nameof(request.Precio)] =
                new[] { $"El precio debe estar entre {PRECIO_MINIMO} y {PRECIO_MAXIMO}." };
        }

        if (request.Stock < STOCK_MINIMO)
        {
            errores[nameof(request.Stock)] = new[] { "El stock no puede ser negativo." };
        }

        return errores;
    }

    /// <summary>Indica si el término de búsqueda tiene la longitud mínima.</summary>
    public static bool EsTerminoValido(string? termino) =>
        !string.IsNullOrWhiteSpace(termino) && termino.Trim().Length >= TERMINO_BUSQUEDA_MIN_LENGTH;

    private static void ValidarTextoObligatorio(
        Dictionary<string, string[]> errores, string campo, string? valor, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            errores[campo] = new[] { $"El campo {campo} es obligatorio." };
        }
        else if (valor.Trim().Length > maxLength)
        {
            errores[campo] = new[] { $"El campo {campo} no puede exceder {maxLength} caracteres." };
        }
    }
}
