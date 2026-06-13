using NetGuardGT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _service;

    public ReportesController(ReporteService service)
    {
        _service = service;
    }

    [HttpGet("incidentes")]
    public async Task<IActionResult> Resumen()
    {
        return Ok(await _service.ObtenerResumenAsync());
    }

    [HttpGet("incidentes-por-estado")]
    public async Task<IActionResult> PorEstado()
    {
        return Ok(await _service.ObtenerPorEstadoAsync());
    }

    [HttpGet("incidentes-por-tecnico")]
    public async Task<IActionResult> PorTecnico()
    {
        return Ok(await _service.ObtenerPorTecnicoAsync());
    }

    [HttpGet("incidentes-escalados")]
    public async Task<IActionResult> Escalados()
    {
        return Ok(await _service.ObtenerEscaladosAsync());
    }
}
