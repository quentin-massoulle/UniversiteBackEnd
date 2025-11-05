namespace UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;

public interface IUeRepository
{
    Task<Ue> CreateAsync(Ue ue);
    Task SaveChangesAsync();
}
