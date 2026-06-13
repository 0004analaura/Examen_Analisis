using NetGuardGT.Api.Data;
using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace NetGuardGT.Api.Services;

public class ReporteService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public ReporteService(AppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<ReporteIncidenteDto> ObtenerResumenAsync()
    {
        var incidentes = await _context.Incidentes.ToListAsync();
        var ahora = _clock.UtcNow;

        var fueraDeSla = incidentes.Count(i =>
            i.Estado != EstadoIncidente.Cerrado
            && i.Estado != EstadoIncidente.Resuelto
            && ahora - i.FechaRegistro > ReglasNegocio.ObtenerTiempoMaximoResolucion(i.Severidad));

        return new ReporteIncidenteDto(
            incidentes.Count,
            incidentes.Count(i => ReglasNegocio.EstadosActivos.Contains(i.Estado)),
            incidentes.Count(i => i.Estado == EstadoIncidente.Escalado),
            incidentes.Count(i => i.Estado == EstadoIncidente.Cerrado),
            fueraDeSla);
    }

    public async Task<List<ReportePorEstadoDto>> ObtenerPorEstadoAsync()
    {
        var incidentes = await _context.Incidentes.ToListAsync();

        return incidentes
            .GroupBy(i => i.Estado)
            .Select(g => new ReportePorEstadoDto(g.Key, g.Count()))
            .OrderBy(r => r.Estado)
            .ToList();
    }

    public async Task<List<ReportePorTecnicoDto>> ObtenerPorTecnicoAsync()
    {
        var incidentes = await _context.Incidentes
            .Include(i => i.Tecnico)
            .ToListAsync();

        return incidentes
            .GroupBy(i => i.Tecnico != null ? i.Tecnico.Nombre : "Sin asignar")
            .Select(g => new ReportePorTecnicoDto(
                g.First().Tecnico?.Id ?? 0,
                g.Key,
                g.Count(),
                g.Count(i => ReglasNegocio.EstadosActivos.Contains(i.Estado))))
            .OrderByDescending(r => r.CantidadIncidentes)
            .ToList();
    }

    public async Task<List<ReporteEscaladosDto>> ObtenerEscaladosAsync()
    {
        var incidentes = await _context.Incidentes
            .Include(i => i.Tecnico)
            .Where(i => i.Estado == EstadoIncidente.Escalado)
            .OrderByDescending(i => i.FechaRegistro)
            .ToListAsync();

        return incidentes
            .Select(i => new ReporteEscaladosDto(
                i.Id,
                i.Titulo,
                i.Severidad,
                i.Sitio,
                i.FechaRegistro,
                i.Tecnico?.Nombre))
            .ToList();
    }
}
