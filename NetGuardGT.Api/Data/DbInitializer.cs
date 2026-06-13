using NetGuardGT.Api.Models;
using NetGuardGT.Api.Models.Enums;

namespace NetGuardGT.Api.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Tecnicos.Any())
            return;

        var tecnicos = new List<Tecnico>
        {
            new() { Nombre = "Carlos Méndez", Especialidad = Especialidad.FibraOptica, Activo = true },
            new() { Nombre = "Ana López", Especialidad = Especialidad.FibraOptica, Activo = true },
            new() { Nombre = "Roberto Paz", Especialidad = Especialidad.FibraOptica, Activo = true },
            new() { Nombre = "María González", Especialidad = Especialidad.Microondas, Activo = true },
            new() { Nombre = "Jorge Ramírez", Especialidad = Especialidad.Microondas, Activo = true },
            new() { Nombre = "Sofía Herrera", Especialidad = Especialidad.Microondas, Activo = true },
            new() { Nombre = "Pedro Castillo", Especialidad = Especialidad.SistemasElectricos, Activo = true },
            new() { Nombre = "Laura Morales", Especialidad = Especialidad.SistemasElectricos, Activo = true },
            new() { Nombre = "Diego Vásquez", Especialidad = Especialidad.SistemasElectricos, Activo = true },
            new() { Nombre = "Elena Ruiz", Especialidad = Especialidad.FibraOptica, Activo = false }
        };

        context.Tecnicos.AddRange(tecnicos);
        context.SaveChanges();

        var ahora = DateTime.UtcNow;

        var incidentes = new List<Incidente>
        {
            new()
            {
                Titulo = "Corte de fibra en Quetzaltenango",
                Descripcion = "Fibra principal dañada por obras en vía pública.",
                Tipo = TipoIncidente.FibraOptica,
                Severidad = Severidad.Critica,
                Estado = EstadoIncidente.Registrado,
                Sitio = "Quetzaltenango Centro",
                FechaRegistro = ahora.AddHours(-3)
            },
            new()
            {
                Titulo = "Enlace microondas degradado",
                Descripcion = "Pérdida de señal en enlace hacia Petén.",
                Tipo = TipoIncidente.Microondas,
                Severidad = Severidad.Media,
                Estado = EstadoIncidente.Asignado,
                Sitio = "Flores Petén",
                FechaRegistro = ahora.AddHours(-5),
                FechaAsignacion = ahora.AddHours(-4),
                TecnicoId = 4
            },
            new()
            {
                Titulo = "Fallo UPS sitio remoto",
                Descripcion = "Sistema eléctrico de respaldo sin carga.",
                Tipo = TipoIncidente.SistemasElectricos,
                Severidad = Severidad.Urgente,
                Estado = EstadoIncidente.EnProgreso,
                Sitio = "Escuintla Sur",
                FechaRegistro = ahora.AddHours(-6),
                FechaAsignacion = ahora.AddHours(-5),
                FechaInicioAtencion = ahora.AddHours(-4),
                TecnicoId = 7
            },
            new()
            {
                Titulo = "Latencia elevada en backbone",
                Descripcion = "Reporte de latencia en nodo capital.",
                Tipo = TipoIncidente.FibraOptica,
                Severidad = Severidad.Baja,
                Estado = EstadoIncidente.Registrado,
                Sitio = "Ciudad de Guatemala Zona 10",
                FechaRegistro = ahora.AddHours(-1)
            },
            new()
            {
                Titulo = "Antena microondas desalineada",
                Descripcion = "Tormenta desalineó antena en sitio alto.",
                Tipo = TipoIncidente.Microondas,
                Severidad = Severidad.Media,
                Estado = EstadoIncidente.Resuelto,
                Sitio = "Huehuetenango",
                FechaRegistro = ahora.AddDays(-2),
                FechaAsignacion = ahora.AddDays(-2).AddHours(1),
                FechaInicioAtencion = ahora.AddDays(-2).AddHours(3),
                FechaResolucion = ahora.AddDays(-1),
                TecnicoId = 5
            }
        };

        context.Incidentes.AddRange(incidentes);
        context.SaveChanges();

        foreach (var incidente in incidentes.Where(i => i.Estado != EstadoIncidente.Registrado))
        {
            context.HistorialEstados.Add(new HistorialEstado
            {
                IncidenteId = incidente.Id,
                EstadoAnterior = EstadoIncidente.Registrado,
                EstadoNuevo = incidente.Estado == EstadoIncidente.Asignado
                    ? EstadoIncidente.Asignado
                    : incidente.Estado,
                FechaCambio = incidente.FechaAsignacion ?? incidente.FechaRegistro,
                Observacion = "Estado inicial de ejemplo",
                TecnicoId = incidente.TecnicoId
            });
        }

        context.SaveChanges();
    }
}
