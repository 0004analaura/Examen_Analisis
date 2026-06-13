using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.Services;

public static class ReglasNegocio
{
    public static readonly EstadoIncidente[] EstadosActivos =
    {
        EstadoIncidente.Asignado,
        EstadoIncidente.EnProgreso,
        EstadoIncidente.Escalado
    };

    public static TimeSpan ObtenerTiempoMaximoResolucion(Severidad severidad) => severidad switch
    {
        Severidad.Baja => TimeSpan.FromHours(48),
        Severidad.Media => TimeSpan.FromHours(24),
        Severidad.Critica => TimeSpan.FromHours(8),
        Severidad.Urgente => TimeSpan.FromHours(4),
        _ => TimeSpan.FromHours(48)
    };

    public static bool EspecialidadCoincide(Especialidad especialidad, TipoIncidente tipo) =>
        especialidad.ToString() == tipo.ToString();

    // Transiciones permitidas sin saltar estados
    public static bool EsTransicionValida(EstadoIncidente actual, EstadoIncidente nuevo)
    {
        if (actual == nuevo)
            return false;

        return (actual, nuevo) switch
        {
            (EstadoIncidente.Registrado, EstadoIncidente.Asignado) => true,
            (EstadoIncidente.Registrado, EstadoIncidente.Escalado) => true,
            (EstadoIncidente.Escalado, EstadoIncidente.Asignado) => true,
            (EstadoIncidente.Asignado, EstadoIncidente.EnProgreso) => true,
            (EstadoIncidente.Asignado, EstadoIncidente.Escalado) => true,
            (EstadoIncidente.EnProgreso, EstadoIncidente.Escalado) => true,
            (EstadoIncidente.EnProgreso, EstadoIncidente.Resuelto) => true,
            (EstadoIncidente.Escalado, EstadoIncidente.EnProgreso) => true,
            (EstadoIncidente.Resuelto, EstadoIncidente.Cerrado) => true,
            _ => false
        };
    }

    public static bool PuedeEscalarPorTiempo(Severidad severidad) =>
        severidad is Severidad.Critica or Severidad.Urgente;
}
