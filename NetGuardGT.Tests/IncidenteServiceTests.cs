using NetGuardGT.Api.Data;
using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Models;
using NetGuardGT.Api.Models.Enums;
using NetGuardGT.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace NetGuardGT.Tests;

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 6, 13, 12, 0, 0, DateTimeKind.Utc);
}

public class TestDbFactory
{
    public static (AppDbContext Context, FakeDateTimeProvider Clock) Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var clock = new FakeDateTimeProvider();
        SeedBaseData(context, clock.UtcNow);
        return (context, clock);
    }

    private static void SeedBaseData(AppDbContext context, DateTime now)
    {
        context.Tecnicos.AddRange(
            new Tecnico { Id = 1, Nombre = "Téc Fibra", Especialidad = Especialidad.FibraOptica, Activo = true },
            new Tecnico { Id = 2, Nombre = "Téc Micro", Especialidad = Especialidad.Microondas, Activo = true },
            new Tecnico { Id = 3, Nombre = "Téc Electrico", Especialidad = Especialidad.SistemasElectricos, Activo = true }
        );
        context.SaveChanges();
    }

    public static Incidente CrearIncidente(
        AppDbContext context,
        TipoIncidente tipo = TipoIncidente.FibraOptica,
        Severidad severidad = Severidad.Media,
        EstadoIncidente estado = EstadoIncidente.Registrado,
        DateTime? fechaRegistro = null,
        int? tecnicoId = null)
    {
        var incidente = new Incidente
        {
            Titulo = "Test incidente",
            Descripcion = "Descripción de prueba",
            Tipo = tipo,
            Severidad = severidad,
            Estado = estado,
            Sitio = "Sitio test",
            FechaRegistro = fechaRegistro ?? DateTime.UtcNow,
            TecnicoId = tecnicoId,
            FechaAsignacion = tecnicoId.HasValue ? DateTime.UtcNow : null
        };
        context.Incidentes.Add(incidente);
        context.SaveChanges();
        return incidente;
    }
}

public class IncidenteServiceTests
{
    private IncidenteService CreateService(AppDbContext context, FakeDateTimeProvider clock) =>
        new(context, clock);

    [Fact]
    public async Task NoPermiteAsignarTecnicoConMasDe3IncidentesActivos()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);
        const int tecnicoId = 1;

        for (var i = 0; i < 3; i++)
        {
            var inc = TestDbFactory.CrearIncidente(context, estado: EstadoIncidente.Asignado, tecnicoId: tecnicoId);
            inc.Estado = EstadoIncidente.Asignado;
        }
        await context.SaveChangesAsync();

        var nuevo = TestDbFactory.CrearIncidente(context);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AsignarAsync(nuevo.Id, tecnicoId));

        Assert.Equal("El técnico ya tiene 3 incidentes activos.", ex.Message);
    }

    [Fact]
    public async Task NoPermiteAsignarTecnicoConEspecialidadIncorrecta()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context, tipo: TipoIncidente.Microondas);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AsignarAsync(incidente.Id, 1)); // Técnico de fibra

        Assert.Equal("La especialidad del técnico no coincide con el tipo de incidente.", ex.Message);
    }

    [Fact]
    public async Task NoPermiteCambioDeEstadoInvalido()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CambiarEstadoAsync(incidente.Id, new CambioEstadoDto(EstadoIncidente.Resuelto, "Salto inválido")));

        Assert.Equal("El cambio de estado no es válido.", ex.Message);
    }

    [Fact]
    public async Task PermiteCambioCorrectoDeRegistradoAAsignado()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context);

        var resultado = await service.AsignarAsync(incidente.Id, 1);

        Assert.Equal(EstadoIncidente.Asignado, resultado.Estado);
        Assert.Equal(1, resultado.TecnicoId);
    }

    [Fact]
    public async Task PermiteCambioCorrectoDeAsignadoAEnProgreso()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context, estado: EstadoIncidente.Asignado, tecnicoId: 1);

        var resultado = await service.CambiarEstadoAsync(
            incidente.Id,
            new CambioEstadoDto(EstadoIncidente.EnProgreso, "Inicio de atención"));

        Assert.Equal(EstadoIncidente.EnProgreso, resultado.Estado);
        Assert.NotNull(resultado.FechaInicioAtencion);
    }

    [Fact]
    public async Task CreaHistorialAlCambiarEstado()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context, estado: EstadoIncidente.Asignado, tecnicoId: 1);
        var historialAntes = await context.HistorialEstados.CountAsync();

        await service.CambiarEstadoAsync(
            incidente.Id,
            new CambioEstadoDto(EstadoIncidente.EnProgreso, "Prueba historial"));

        var historialDespues = await context.HistorialEstados.CountAsync();
        Assert.True(historialDespues > historialAntes);

        var ultimo = await context.HistorialEstados.OrderByDescending(h => h.Id).FirstAsync();
        Assert.Equal(EstadoIncidente.Asignado, ultimo.EstadoAnterior);
        Assert.Equal(EstadoIncidente.EnProgreso, ultimo.EstadoNuevo);
    }

    [Fact]
    public async Task EscalaIncidenteCriticoDespuesDe2HorasSinAtencion()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        clock.UtcNow = new DateTime(2026, 6, 13, 15, 0, 0, DateTimeKind.Utc);
        TestDbFactory.CrearIncidente(
            context,
            severidad: Severidad.Critica,
            estado: EstadoIncidente.Registrado,
            fechaRegistro: clock.UtcNow.AddHours(-3));

        var resultado = await service.RevisarEscalacionAsync();

        Assert.Equal(1, resultado.IncidentesEscalados);
        Assert.All(resultado.Incidentes, i => Assert.Equal(EstadoIncidente.Escalado, i.Estado));
    }

    [Fact]
    public async Task EscalaIncidenteUrgenteDespuesDe2HorasSinAtencion()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        clock.UtcNow = new DateTime(2026, 6, 13, 10, 0, 0, DateTimeKind.Utc);
        TestDbFactory.CrearIncidente(
            context,
            severidad: Severidad.Urgente,
            estado: EstadoIncidente.Registrado,
            fechaRegistro: clock.UtcNow.AddHours(-2.5));

        var resultado = await service.RevisarEscalacionAsync();

        Assert.Equal(1, resultado.IncidentesEscalados);
    }

    [Fact]
    public async Task NoEscalaIncidenteDeSeveridadBaja()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        clock.UtcNow = new DateTime(2026, 6, 13, 18, 0, 0, DateTimeKind.Utc);
        TestDbFactory.CrearIncidente(
            context,
            severidad: Severidad.Baja,
            estado: EstadoIncidente.Registrado,
            fechaRegistro: clock.UtcNow.AddHours(-10));

        var resultado = await service.RevisarEscalacionAsync();

        Assert.Equal(0, resultado.IncidentesEscalados);
    }

    [Fact]
    public async Task NoPermiteModificarIncidenteCerrado()
    {
        var (context, clock) = TestDbFactory.Create();
        var service = CreateService(context, clock);

        var incidente = TestDbFactory.CrearIncidente(context, estado: EstadoIncidente.Cerrado, tecnicoId: 1);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CambiarEstadoAsync(incidente.Id, new CambioEstadoDto(EstadoIncidente.Resuelto, "Intento inválido")));

        Assert.Equal("No se puede modificar un incidente cerrado.", ex.Message);
    }
}
