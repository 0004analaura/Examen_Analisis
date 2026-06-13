using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentesController : ControllerBase
{
    private readonly IncidenteService _service;

    public IncidentesController(IncidenteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<IncidenteResponseDto>>> GetAll()
    {
        return Ok(await _service.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncidenteResponseDto>> GetById(int id)
    {
        var incidente = await _service.ObtenerPorIdAsync(id);
        if (incidente is null)
            return NotFound(new { mensaje = "Incidente no encontrado." });

        return Ok(incidente);
    }

    [HttpPost]
    public async Task<ActionResult<IncidenteResponseDto>> Create(IncidenteCreateDto dto)
    {
        var creado = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}/asignar/{tecnicoId}")]
    public async Task<ActionResult<IncidenteResponseDto>> Asignar(int id, int tecnicoId)
    {
        try
        {
            return Ok(await _service.AsignarAsync(id, tecnicoId));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/reasignar/{tecnicoId}")]
    public async Task<ActionResult<IncidenteResponseDto>> Reasignar(int id, int tecnicoId)
    {
        try
        {
            return Ok(await _service.ReasignarAsync(id, tecnicoId));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<IncidenteResponseDto>> CambiarEstado(int id, CambioEstadoDto dto)
    {
        try
        {
            return Ok(await _service.CambiarEstadoAsync(id, dto));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("escalar")]
    public async Task<ActionResult<EscalacionResultadoDto>> Escalar()
    {
        return Ok(await _service.RevisarEscalacionAsync());
    }

    [HttpGet("{id}/historial")]
    public async Task<ActionResult<List<HistorialEstadoDto>>> Historial(int id)
    {
        try
        {
            return Ok(await _service.ObtenerHistorialAsync(id));
        }
        catch (BusinessRuleException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
