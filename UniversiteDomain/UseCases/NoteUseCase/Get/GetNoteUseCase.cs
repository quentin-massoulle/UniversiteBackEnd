using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;

namespace UniversiteDomain.UseCases.NoteUseCase.Get;

public class GetNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<List<Note>> ExecuteAsync()
    {
        return await repositoryFactory.NoteRepository().FindAllAsync();
    }

    public async Task<Note?> ExecuteAsync(long etudiantId, long ueId)
    {
        return await repositoryFactory.NoteRepository().FindAsync(etudiantId, ueId);
    }
}
