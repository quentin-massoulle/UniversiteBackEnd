namespace UniversiteDomain.DataAdapters;
using System.Linq.Expressions;
using UniversiteDomain.Entites;

public interface IUeRepository
{
    Task<Ue> CreateAsync(Ue ue);
    Task<List<Ue>> FindByConditionAsync(Expression<Func<Ue, bool>> predicate);
    Task SaveChangesAsync();
}
