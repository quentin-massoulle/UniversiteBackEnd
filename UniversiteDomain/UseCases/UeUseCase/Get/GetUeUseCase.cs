using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCase.Get;

public class GetUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<List<Ue>> ExecuteAsync()
    {
        return await repositoryFactory.UeRepository().FindAllAsync();
    }

    public async Task<Ue> ExecuteAsync(long id)
    {
        Ue? ue = await repositoryFactory.UeRepository().FindAsync(id);
        if (ue == null)
        {
            throw new UeNotFoundException($"L'Unité d'Enseignement avec l'ID {id} n'a pas été trouvée.");
        }
        return ue;
    }
}