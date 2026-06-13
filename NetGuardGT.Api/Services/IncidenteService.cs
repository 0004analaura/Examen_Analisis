using NetGuardGT.Api.Data;
using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Models;
using NetGuardGT.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace NetGuardGT.Api.Services;

public class IncidenteService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public IncidenteService(AppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<List<IncidenteResponseDto>> ObtenerTodosAsync()
    {
        var incidentes = await QueryConTecnico().OrderByDescending(i => i.FechaRegistro).ToListAsync();
        return incidentes.Select(MapToDto).ToList();
    }

    public async Task<IncidenteResponseDto?> ObtenerPorIdAsync(int id)
    {
        var incidente = await QueryConTecnico().FirstOrDefaultAsync(i => i.Id == id);
        return incidente is null ? null : MapToDto(incidente);
    }

    public async Task<IncidenteResponseDto> CrearAsync(IncidenteCreateDto dto)
    {
        var incidente = new Incidente
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Tipo = dto.Tipo,
            Severidad = dto.Severidad,
            Sitio = dto.Sitio,
            Estado = EstadoIncidente.Registrado,
            FechaRegistro = _clock.UtcNow
        };

        _context.Incidentes.Add(incidente);
        await _context.SaveChangesAsync();

        await RegistrarHistorialAsync(incidente, EstadoIncidente.Registrado, EstadoIncidente.Registrado,
            "Incidente registrado en el sistema", null);

        return MapToDto(incidente);
    }

    public async Task<IncidenteResponseDto> AsignarAsync(int incidenteId, int tecnicoId)
    {
        var incidente = await ObtenerIncidenteParaModificarAsync(incidenteId);
        var tecnico = await ObtenerTecnicoValidoAsync(tecnicoId);

        if (incidente.Estado is not (EstadoIncidente.Registrado or EstadoIncidente.Escalado))
            throw new BusinessRuleException("Solo se puede asignar un incidente en estado Registrado o Escalado.");

        ValidarEspecialidad(tecnico, incidente);
        await ValidarCargaTecnicoAsync(tecnicoId, incidenteId);

        var estadoAnterior = incidente.Estado;
        incidente.TecnicoId = tecnicoId;
        incidente.FechaAsignacion = _clock.UtcNow;
        await CambiarEstadoInternoAsync(incidente, EstadoIncidente.Asignado, "Técnico asignado al incidente", tecnicoId, estadoAnterior);

        return MapToDto(incidente);
    }

    public async Task<IncidenteResponseDto> ReasignarAsync(int incidenteId, int tecnicoId)
    {
        var incidente = await ObtenerIncidenteParaModificarAsync(incidenteId);
        var tecnico = await ObtenerTecnicoValidoAsync(tecnicoId);

        if (incidente.Estado == EstadoIncidente.Cerrado)
            throw new BusinessRuleException("No se puede modificar un incidente cerrado.");

        if (incidente.TecnicoId == tecnicoId)
            throw new BusinessRuleException("El incidente ya está asignado a este técnico.");

        ValidarEspecialidad(tecnico, incidente);
        await ValidarCargaTecnicoAsync(tecnicoId, incidenteId);

        var tecnicoAnterior = incidente.TecnicoId;
        incidente.TecnicoId = tecnicoId;
        incidente.FechaAsignacion = _clock.UtcNow;

        if (incidente.Estado == EstadoIncidente.Registrado)
            incidente.Estado = EstadoIncidente.Asignado;

        await RegistrarHistorialAsync(
            incidente,
            incidente.Estado,
            incidente.Estado,
            $"Incidente reasignado del técnico {tecnicoAnterior?.ToString() ?? "N/A"} al técnico {tecnicoId}",
            tecnicoId);

        await _context.SaveChangesAsync();
        await _context.Entry(incidente).Reference(i => i.Tecnico).LoadAsync();

        return MapToDto(incidente);
    }

    public async Task<IncidenteResponseDto> CambiarEstadoAsync(int incidenteId, CambioEstadoDto dto)
    {
        var incidente = await ObtenerIncidenteParaModificarAsync(incidenteId);
        var estadoAnterior = incidente.Estado;

        if (!ReglasNegocio.EsTransicionValida(estadoAnterior, dto.EstadoNuevo))
            throw new BusinessRuleException("El cambio de estado no es válido.");

        // Asignado requiere técnico asignado
        if (dto.EstadoNuevo == EstadoIncidente.Asignado && incidente.TecnicoId is null)
            throw new BusinessRuleException("Debe asignar un técnico antes de cambiar a estado Asignado.");

        await CambiarEstadoInternoAsync(incidente, dto.EstadoNuevo, dto.Observacion, incidente.TecnicoId, estadoAnterior);
        return MapToDto(incidente);
    }

    public async Task<EscalacionResultadoDto> RevisarEscalacionAsync()
    {
        var limite = _clock.UtcNow.AddHours(-2);

        var candidatos = await _context.Incidentes
            .Include(i => i.Tecnico)
            .Where(i => i.Estado == EstadoIncidente.Registrado
                        && i.FechaRegistro <= limite
                        && (i.Severidad == Severidad.Critica || i.Severidad == Severidad.Urgente))
            .ToListAsync();

        var escalados = new List<IncidenteResponseDto>();

        foreach (var incidente in candidatos)
        {
            await CambiarEstadoInternoAsync(
                incidente,
                EstadoIncidente.Escalado,
                "Escalado automático: más de 2 horas sin atención",
                null,
                incidente.Estado);

            escalados.Add(MapToDto(incidente));
        }

        return new EscalacionResultadoDto(escalados.Count, escalados);
    }

    public async Task<List<HistorialEstadoDto>> ObtenerHistorialAsync(int incidenteId)
    {
        var existe = await _context.Incidentes.AnyAsync(i => i.Id == incidenteId);
        if (!existe)
            throw new BusinessRuleException("Incidente no encontrado.");

        return await _context.HistorialEstados
            .Include(h => h.Tecnico)
            .Where(h => h.IncidenteId == incidenteId)
            .OrderBy(h => h.FechaCambio)
            .Select(h => new HistorialEstadoDto(
                h.Id,
                h.IncidenteId,
                h.EstadoAnterior,
                h.EstadoNuevo,
                h.FechaCambio,
                h.Observacion,
                h.TecnicoId,
                h.Tecnico != null ? h.Tecnico.Nombre : null))
            .ToListAsync();
    }

    // --- Métodos internos ---

    private IQueryable<Incidente> QueryConTecnico() =>
        _context.Incidentes.Include(i => i.Tecnico);

    private async Task<Incidente> ObtenerIncidenteParaModificarAsync(int id)
    {
        var incidente = await _context.Incidentes
            .Include(i => i.Tecnico)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incidente is null)
            throw new BusinessRuleException("Incidente no encontrado.");

        if (incidente.Estado == EstadoIncidente.Cerrado)
            throw new BusinessRuleException("No se puede modificar un incidente cerrado.");

        return incidente;
    }

    private async Task<Tecnico> ObtenerTecnicoValidoAsync(int tecnicoId)
    {
        var tecnico = await _context.Tecnicos.FindAsync(tecnicoId);
        if (tecnico is null)
            throw new BusinessRuleException("Técnico no encontrado.");

        if (!tecnico.Activo)
            throw new BusinessRuleException("El técnico no está activo.");

        return tecnico;
    }

    private static void ValidarEspecialidad(Tecnico tecnico, Incidente incidente)
    {
        if (!ReglasNegocio.EspecialidadCoincide(tecnico.Especialidad, incidente.Tipo))
            throw new BusinessRuleException("La especialidad del técnico no coincide con el tipo de incidente.");
    }

    private async Task ValidarCargaTecnicoAsync(int tecnicoId, int incidenteExcluido)
    {
        var activos = await _context.Incidentes.CountAsync(i =>
            i.TecnicoId == tecnicoId
            && i.Id != incidenteExcluido
            && ReglasNegocio.EstadosActivos.Contains(i.Estado));

        if (activos >= 3)
            throw new BusinessRuleException("El técnico ya tiene 3 incidentes activos.");
    }

    private async Task CambiarEstadoInternoAsync(
        Incidente incidente,
        EstadoIncidente nuevoEstado,
        string observacion,
        int? tecnicoId,
        EstadoIncidente estadoAnterior)
    {
        incidente.Estado = nuevoEstado;

        if (nuevoEstado == EstadoIncidente.EnProgreso && incidente.FechaInicioAtencion is null)
            incidente.FechaInicioAtencion = _clock.UtcNow;

        if (nuevoEstado == EstadoIncidente.Resuelto)
            incidente.FechaResolucion = _clock.UtcNow;

        await RegistrarHistorialAsync(incidente, estadoAnterior, nuevoEstado, observacion, tecnicoId);
        await _context.SaveChangesAsync();
    }

    private async Task RegistrarHistorialAsync(
        Incidente incidente,
        EstadoIncidente anterior,
        EstadoIncidente nuevo,
        string observacion,
        int? tecnicoId)
    {
        _context.HistorialEstados.Add(new HistorialEstado
        {
            IncidenteId = incidente.Id,
            EstadoAnterior = anterior,
            EstadoNuevo = nuevo,
            FechaCambio = _clock.UtcNow,
            Observacion = observacion,
            TecnicoId = tecnicoId
        });

        if (incidente.Id == 0)
            await _context.SaveChangesAsync();
    }

    internal static IncidenteResponseDto MapToDto(Incidente incidente) =>
        new(
            incidente.Id,
            incidente.Titulo,
            incidente.Descripcion,
            incidente.Tipo,
            incidente.Severidad,
            incidente.Estado,
            incidente.Sitio,
            incidente.FechaRegistro,
            incidente.FechaAsignacion,
            incidente.FechaInicioAtencion,
            incidente.FechaResolucion,
            incidente.TecnicoId,
            incidente.Tecnico?.Nombre,
            ReglasNegocio.ObtenerTiempoMaximoResolucion(incidente.Severidad).TotalHours);
}
