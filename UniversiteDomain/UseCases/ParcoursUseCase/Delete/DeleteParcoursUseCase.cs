using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.ParcoursUseCase.Delete;

public class DeleteParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long id)
    {
        await repositoryFactory.ParcoursRepository().DeleteAsync(id);
        await repositoryFactory.SaveChangesAsync();
    }
}
