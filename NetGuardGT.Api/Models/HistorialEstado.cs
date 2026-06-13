using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.Models;

public class HistorialEstado
{
    public int Id { get; set; }
    public int IncidenteId { get; set; }
    public EstadoIncidente EstadoAnterior { get; set; }
    public EstadoIncidente EstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public int? TecnicoId { get; set; }

    public Incidente? Incidente { get; set; }
    public Tecnico? Tecnico { get; set; }
}
