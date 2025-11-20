using Microsoft.Extensions.Logging;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;


namespace UniversiteEFDataProvider.Repositories;

public class UeRepository (UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task<Ue?> GetByIdAsync(long id)
    {
        return await context.Ues.FindAsync(id); 
       
    }
}