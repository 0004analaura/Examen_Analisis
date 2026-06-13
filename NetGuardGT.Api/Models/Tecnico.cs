using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.Models;

public class Tecnico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Especialidad Especialidad { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();
}
