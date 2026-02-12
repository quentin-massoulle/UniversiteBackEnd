using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;


namespace UniversiteEFDataProvider.Repositories;

public class UeRepository (UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task<Ue?> GetByIdAsync(long id)
    {
        return await context.Ues
            .Include(u => u.Notes)
            .Include(u => u.EnseigneeDans)
            .FirstOrDefaultAsync(u => u.Id == id); 
    }

    public async Task<List<Ue>> GetAllAsync()
    {
        return await context.Ues
            .Include(u => u.Notes)
            .Include(u => u.EnseigneeDans)
            .ToListAsync();
    }

    public async Task<Ue> AddParcoursAsync(long idUe, long idParcours)
    {
        Ue? ue = await context.Ues.Include(u => u.EnseigneeDans).FirstOrDefaultAsync(u => u.Id == idUe);
        Parcours? parcours = await context.Parcours.FindAsync(idParcours);
        
        if (ue == null || parcours == null) throw new NullReferenceException();
        
        if (ue.EnseigneeDans == null) ue.EnseigneeDans = new List<Parcours>();
        
        if (!ue.EnseigneeDans.Contains(parcours))
        {
            ue.EnseigneeDans.Add(parcours);
            await context.SaveChangesAsync();
        }
        return ue;
    }

    public async Task<Ue> AddParcoursAsync(long idUe, long[] idParcours)
    {
        Ue? ue = await context.Ues.Include(u => u.EnseigneeDans).FirstOrDefaultAsync(u => u.Id == idUe);
        if (ue == null) throw new NullReferenceException();
        
        if (ue.EnseigneeDans == null) ue.EnseigneeDans = new List<Parcours>();

        foreach (var id in idParcours)
        {
            Parcours? parcours = await context.Parcours.FindAsync(id);
            if (parcours != null && !ue.EnseigneeDans.Contains(parcours))
            {
                ue.EnseigneeDans.Add(parcours);
            }
        }
        await context.SaveChangesAsync();
        return ue;
    }

    public async Task<Ue?> FindUeCompletAsync(long idUe)
    {
        return await context.Ues
            .Include(u => u.EnseigneeDans)
            .ThenInclude(p => p.Inscrits)
            .Include(u => u.Notes)
            .FirstOrDefaultAsync(u => u.Id == idUe);
    }

    public async Task<Ue?> FindUeCompletByNumeroAsync(string numeroUe)
    {
        return await context.Ues
            .Include(u => u.EnseigneeDans)
            .ThenInclude(p => p.Inscrits)
            .Include(u => u.Notes)
            .FirstOrDefaultAsync(u => u.NumeroUe == numeroUe);
    }

    public async Task<List<Ue>> FindAllUesCompletAsync()
    {
        return await context.Ues
            .Include(u => u.EnseigneeDans)
            .ThenInclude(p => p.Inscrits)
            .Include(u => u.Notes)
            .ToListAsync();
    }
}