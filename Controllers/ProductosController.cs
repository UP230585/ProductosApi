using Microsoft.AspNetCore.Mvc;
using ProductosApi.Data;
using ProductosApi.Models;

namespace ProductosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _repositorio;

    public ProductosController(IProductoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    [HttpGet]
    public ActionResult<List<Producto>> ObtenerTodos() => _repositorio.ObtenerTodos();

    [HttpGet("{id:int}")]
    public ActionResult<Producto> ObtenerPorId(int id)
    {
        var producto = _repositorio.ObtenerPorId(id);
        return producto is null ? NotFound() : producto;
    }

    [HttpGet("buscar")]
    public ActionResult<List<Producto>> Buscar([FromQuery] string texto) => _repositorio.Buscar(texto);

    [HttpPost]
    public ActionResult<Producto> Insertar(Producto producto)
    {
        producto.Id = _repositorio.Insertar(producto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, Producto producto)
    {
        producto.Id = id;
        return _repositorio.Actualizar(producto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id) => _repositorio.Eliminar(id) ? NoContent() : NotFound();
}
