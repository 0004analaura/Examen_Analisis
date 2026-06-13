using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.Models;

public class Incidente
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoIncidente Tipo { get; set; }
    public Severidad Severidad { get; set; }
    public EstadoIncidente Estado { get; set; } = EstadoIncidente.Registrado;
    public string Sitio { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaInicioAtencion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public int? TecnicoId { get; set; }

    public Tecnico? Tecnico { get; set; }
    public ICollection<HistorialEstado> Historial { get; set; } = new List<HistorialEstado>();
}
