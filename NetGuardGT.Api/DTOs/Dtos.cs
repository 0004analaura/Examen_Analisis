using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.DTOs;

public record TecnicoCreateDto(string Nombre, Especialidad Especialidad, bool Activo = true);
public record TecnicoUpdateDto(string Nombre, Especialidad Especialidad, bool Activo);
public record TecnicoResponseDto(int Id, string Nombre, Especialidad Especialidad, bool Activo, int IncidentesActivos);

public record IncidenteCreateDto(
    string Titulo,
    string Descripcion,
    TipoIncidente Tipo,
    Severidad Severidad,
    string Sitio);

public record IncidenteResponseDto(
    int Id,
    string Titulo,
    string Descripcion,
    TipoIncidente Tipo,
    Severidad Severidad,
    EstadoIncidente Estado,
    string Sitio,
    DateTime FechaRegistro,
    DateTime? FechaAsignacion,
    DateTime? FechaInicioAtencion,
    DateTime? FechaResolucion,
    int? TecnicoId,
    string? TecnicoNombre,
    double HorasMaxResolucion);

public record CambioEstadoDto(EstadoIncidente EstadoNuevo, string Observacion);

public record HistorialEstadoDto(
    int Id,
    int IncidenteId,
    EstadoIncidente EstadoAnterior,
    EstadoIncidente EstadoNuevo,
    DateTime FechaCambio,
    string Observacion,
    int? TecnicoId,
    string? TecnicoNombre);

public record ReporteIncidenteDto(
    int TotalIncidentes,
    int Activos,
    int Escalados,
    int Cerrados,
    int FueraDeSla);

public record ReportePorEstadoDto(EstadoIncidente Estado, int Cantidad);
public record ReportePorTecnicoDto(int TecnicoId, string Nombre, int CantidadIncidentes, int Activos);
public record ReporteEscaladosDto(int Id, string Titulo, Severidad Severidad, string Sitio, DateTime FechaRegistro, string? TecnicoNombre);

public record EscalacionResultadoDto(int IncidentesEscalados, IEnumerable<IncidenteResponseDto> Incidentes);
