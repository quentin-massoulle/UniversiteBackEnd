namespace UniversiteDomain.DataAdapters;
using System.Linq.Expressions;
using UniversiteDomain.Entites;

public interface IUniversiteRoleRepository : IRepository<IUniversiteRole>
{
    Task AddRoleAsync(string role);
}