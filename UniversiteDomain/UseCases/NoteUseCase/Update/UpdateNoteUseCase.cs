using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.NoteExeptions;

namespace UniversiteDomain.UseCases.NoteUseCase.Update;

public class UpdateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long etudiantId, long ueId, float valeur)
    {
        var note = await repositoryFactory.NoteRepository().FindAsync(etudiantId, ueId);
        if (note == null)
        {
            throw new NoteNotFoundException($"La note pour l'étudiant {etudiantId} et l'UE {ueId} n'existe pas.");
        }

        note.Valeur = valeur;
        
        if (note.Valeur < 0 || note.Valeur > 20)
        {
            throw new InvalidValeurNoteException("La note doit être comprise entre 0 et 20.");
        }

        await repositoryFactory.NoteRepository().UpdateAsync(note);
        await repositoryFactory.SaveChangesAsync();

        return note;
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        return await ExecuteAsync(note.EtudiantId, note.UeId, note.Valeur);
    }
}
