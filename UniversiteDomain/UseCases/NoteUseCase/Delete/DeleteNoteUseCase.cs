using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.NoteUseCase.Delete;

public class DeleteNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long etudiantId, long ueId)
    {
        var note = await repositoryFactory.NoteRepository().FindAsync(etudiantId, ueId);
        if (note != null)
        {
            await repositoryFactory.NoteRepository().DeleteAsync(note);
            await repositoryFactory.SaveChangesAsync();
        }
    }
}
