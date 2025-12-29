using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.NoteUseCase.Delete;

public class DeleteNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long id)
    {
        await repositoryFactory.NoteRepository().DeleteAsync(id);
        await repositoryFactory.SaveChangesAsync();
    }
}
