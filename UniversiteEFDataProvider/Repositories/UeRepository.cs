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
}