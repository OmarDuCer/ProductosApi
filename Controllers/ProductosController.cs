using Microsoft.AspNetCore.Mvc;
using ProductosApi.Models;
using ProductosApi.Repositories;
using ProductosApi.Validation;

namespace ProductosApi.Controllers;

/// <summary>
/// Endpoints HTTP del módulo de productos. El controlador NO escribe SQL:
/// valida la petición, llama al repositorio y traduce el resultado a un código HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _repositorio;
    private readonly IProductoBusquedaRepository _busqueda;

    public ProductosController(IProductoRepository repositorio, IProductoBusquedaRepository busqueda)
    {
        _repositorio = repositorio;
        _busqueda = busqueda;
    }

    /// <summary>Consulta todos los productos.</summary>
    /// <response code="200">Lista de productos (puede venir vacía).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Producto>), StatusCodes.Status200OK)]
    public ActionResult<List<Producto>> ObtenerTodos()
    {
        return Ok(_repositorio.ObtenerTodos());
    }

    /// <summary>Consulta un producto por su id.</summary>
    /// <param name="id">Id del producto.</param>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="404">No existe un producto con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<Producto> ObtenerPorId(int id)
    {
        var producto = _repositorio.ObtenerPorId(id);
        return producto is null ? NoEncontrado(id) : Ok(producto);
    }

    /// <summary>Busca productos por nombre, descripción o categoría.</summary>
    /// <param name="termino">Texto a buscar (mínimo 2 caracteres). Ej: "tecla".</param>
    /// <response code="200">Productos que coinciden (puede venir vacía).</response>
    /// <response code="400">Término vacío o demasiado corto.</response>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(List<Producto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<List<Producto>> Buscar([FromQuery] string? termino)
    {
        if (!ProductoValidator.EsTerminoValido(termino))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Término de búsqueda inválido",
                detail: $"El término debe tener al menos {ProductoValidator.TERMINO_BUSQUEDA_MIN_LENGTH} caracteres.");
        }

        return Ok(_busqueda.Buscar(termino!));
    }

    /// <summary>Registra un nuevo producto.</summary>
    /// <response code="201">Producto creado; el encabezado Location apunta a su URL.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<Producto> Insertar([FromBody] ProductoRequest request)
    {
        var errores = ProductoValidator.Validar(request);
        if (errores.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(errores));
        }

        var nuevoId = _repositorio.Insertar(request.ToProducto());

        // Se vuelve a leer para devolver también la fecha que asignó MySQL.
        var creado = _repositorio.ObtenerPorId(nuevoId);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, creado);
    }

    /// <summary>Actualiza todos los datos de un producto existente.</summary>
    /// <param name="id">Id del producto a actualizar.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe un producto con ese id.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<Producto> Actualizar(int id, [FromBody] ProductoRequest request)
    {
        var errores = ProductoValidator.Validar(request);
        if (errores.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(errores));
        }

        var actualizado = _repositorio.Actualizar(request.ToProducto(id));
        if (!actualizado)
        {
            return NoEncontrado(id);
        }

        return Ok(_repositorio.ObtenerPorId(id));
    }

    /// <summary>Elimina un producto.</summary>
    /// <param name="id">Id del producto a eliminar.</param>
    /// <response code="204">Eliminado correctamente.</response>
    /// <response code="404">No existe un producto con ese id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Eliminar(int id)
    {
        return _repositorio.Eliminar(id) ? NoContent() : NoEncontrado(id);
    }

    // ==========================================================
    // HELPERS
    // ==========================================================

    private ObjectResult NoEncontrado(int id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Producto no encontrado",
        detail: $"No existe un producto con id {id}.");
}
