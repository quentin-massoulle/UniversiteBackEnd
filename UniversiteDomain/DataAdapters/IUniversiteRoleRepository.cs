using UniversiteDomain.Entites;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteRoleRepository : IRepository<IUniversiteRole>
{
    Task<bool> AddRoleAsync(string role);
}
