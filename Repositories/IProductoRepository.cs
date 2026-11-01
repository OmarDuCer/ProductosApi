using ProductosApi.Models;

namespace ProductosApi.Repositories;

/// <summary>
/// Interfaz solicitada en la actividad (se respeta tal cual).
/// </summary>
public interface IProductoRepository
{
    List<Producto> ObtenerTodos();
    Producto? ObtenerPorId(int id);
    int Insertar(Producto producto);
    bool Actualizar(Producto producto);
    bool Eliminar(int id);
}

/// <summary>
/// Extensión para el punto "Buscar": búsqueda por texto en nombre, descripción o categoría.
/// Se separa para no modificar la interfaz original.
/// </summary>
public interface IProductoBusquedaRepository
{
    List<Producto> Buscar(string termino);
}
