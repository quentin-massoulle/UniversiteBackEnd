namespace UniversiteDomain.DataAdapters;
using System.Linq.Expressions;
using UniversiteDomain.Entites;

public interface IUeRepository : IRepository<Ue>
{
    Task<Ue?> GetByIdAsync(long id);
    Task<List<Ue>> GetAllAsync();
}
