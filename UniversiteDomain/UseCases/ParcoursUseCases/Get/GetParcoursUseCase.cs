using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;

namespace UniversiteDomain.UseCases.ParcoursUseCase.Get;

public class GetParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<List<Parcours>> ExecuteAsync()
    {
        return await repositoryFactory.ParcoursRepository().FindAllAsync();
    }

    public async Task<Parcours?> ExecuteAsync(long id)
    {
        return await repositoryFactory.ParcoursRepository().FindAsync(id);
    }
}
