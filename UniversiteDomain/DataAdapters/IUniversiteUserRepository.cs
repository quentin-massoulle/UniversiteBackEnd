using UniversiteDomain.Entites;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteUserRepository : IRepository<IUniversiteUser>
{
    Task<IUniversiteUser> AddUserAsync(string login, string email, string password, long? etudiantId = null);
    Task<bool> AddToRoleAsync(IUniversiteUser user, string role);
    Task<IUniversiteUser?> FindByEmailAsync(string email);
}
