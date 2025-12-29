using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.UeUseCase.Delete;

public class DeleteUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long id)
    {
        await repositoryFactory.UeRepository().DeleteAsync(id);
        await repositoryFactory.SaveChangesAsync();
    }
}
