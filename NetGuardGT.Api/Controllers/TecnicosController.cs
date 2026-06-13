using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TecnicosController : ControllerBase
{
    private readonly TecnicoService _service;

    public TecnicosController(TecnicoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TecnicoResponseDto>>> GetAll()
    {
        return Ok(await _service.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TecnicoResponseDto>> GetById(int id)
    {
        var tecnico = await _service.ObtenerPorIdAsync(id);
        if (tecnico is null)
            return NotFound(new { mensaje = "Técnico no encontrado." });

        return Ok(tecnico);
    }

    [HttpPost]
    public async Task<ActionResult<TecnicoResponseDto>> Create(TecnicoCreateDto dto)
    {
        var creado = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TecnicoResponseDto>> Update(int id, TecnicoUpdateDto dto)
    {
        var actualizado = await _service.ActualizarAsync(id, dto);
        if (actualizado is null)
            return NotFound(new { mensaje = "Técnico no encontrado." });

        return Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.EliminarAsync(id);
        if (!eliminado)
            return NotFound(new { mensaje = "Técnico no encontrado." });

        return NoContent();
    }
}
