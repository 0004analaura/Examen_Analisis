using NetGuardGT.Api.Data;
using NetGuardGT.Api.DTOs;
using NetGuardGT.Api.Models;
using NetGuardGT.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace NetGuardGT.Api.Services;

public class TecnicoService
{
    private readonly AppDbContext _context;

    public TecnicoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TecnicoResponseDto>> ObtenerTodosAsync()
    {
        var tecnicos = await _context.Tecnicos
            .Include(t => t.Incidentes)
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        return tecnicos.Select(MapToDto).ToList();
    }

    public async Task<TecnicoResponseDto?> ObtenerPorIdAsync(int id)
    {
        var tecnico = await _context.Tecnicos
            .Include(t => t.Incidentes)
            .FirstOrDefaultAsync(t => t.Id == id);

        return tecnico is null ? null : MapToDto(tecnico);
    }

    public async Task<TecnicoResponseDto> CrearAsync(TecnicoCreateDto dto)
    {
        var tecnico = new Tecnico
        {
            Nombre = dto.Nombre,
            Especialidad = dto.Especialidad,
            Activo = dto.Activo
        };

        _context.Tecnicos.Add(tecnico);
        await _context.SaveChangesAsync();
        return MapToDto(tecnico);
    }

    public async Task<TecnicoResponseDto?> ActualizarAsync(int id, TecnicoUpdateDto dto)
    {
        var tecnico = await _context.Tecnicos
            .Include(t => t.Incidentes)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tecnico is null)
            return null;

        tecnico.Nombre = dto.Nombre;
        tecnico.Especialidad = dto.Especialidad;
        tecnico.Activo = dto.Activo;

        await _context.SaveChangesAsync();
        return MapToDto(tecnico);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var tecnico = await _context.Tecnicos.FindAsync(id);
        if (tecnico is null)
            return false;

        _context.Tecnicos.Remove(tecnico);
        await _context.SaveChangesAsync();
        return true;
    }

    private static TecnicoResponseDto MapToDto(Tecnico tecnico)
    {
        var activos = tecnico.Incidentes.Count(i => ReglasNegocio.EstadosActivos.Contains(i.Estado));
        return new TecnicoResponseDto(tecnico.Id, tecnico.Nombre, tecnico.Especialidad, tecnico.Activo, activos);
    }
}
