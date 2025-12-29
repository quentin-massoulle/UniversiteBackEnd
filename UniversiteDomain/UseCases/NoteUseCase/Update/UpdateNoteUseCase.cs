using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.NoteExceptions;

namespace UniversiteDomain.UseCases.NoteUseCase.Update;

public class UpdateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long id, float valeur)
    {
        var note = await repositoryFactory.NoteRepository().FindAsync(id);
        if (note == null)
        {
            throw new NoteNotFoundException($"La note avec l'ID {id} n'existe pas.");
        }

        note.Valeur = valeur;
        
        if (note.Valeur < 0 || note.Valeur > 20)
        {
            throw new InvalidNoteException("La note doit être comprise entre 0 et 20.");
        }

        await repositoryFactory.NoteRepository().UpdateAsync(note);
        await repositoryFactory.SaveChangesAsync();

        return note;
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        return await ExecuteAsync(note.Id, note.Valeur);
    }
}
