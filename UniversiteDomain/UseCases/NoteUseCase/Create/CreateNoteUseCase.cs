using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.NoteUseCase.Create;

public class CreateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(float valeur, long etudiantId, long ueId)
    {
        var note = new Note { Valeur = valeur, EtudiantId = etudiantId, UeId = ueId };
        return await ExecuteAsync(note);
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        await CheckBusinessRules(note);
        Note created = await repositoryFactory.NoteRepository().CreateAsync(note);
        await repositoryFactory.SaveChangesAsync();
        return created;
    }

    private async Task CheckBusinessRules(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // Vérifie la note est comprise entre 0 et 20
        if (note.Valeur < 0 || note.Valeur > 20)
            throw new InvalidValeurNoteException(
                note.Valeur + " la note doit etre comprise entre 0 et 20 "
            );

        // Doit vérifier si l'UE existe
        Ue? ueExistante = await repositoryFactory.UeRepository().FindAsync(note.UeId);
        if (ueExistante == null)
            throw new InvalidIdUe(
                note.UeId + " l'ue n'existe pas "
            );

        // Doit vérifier si l'étudiant existe
        Etudiant? etudiantExiste = await repositoryFactory.EtudiantRepository().FindAsync(note.EtudiantId);
        if (etudiantExiste == null)
            throw new EtudiantNotFoundException(
                note.EtudiantId + " l'etudiant n'existe pas "
            );

        // Vérification si l'étudiant a déjà une note pour cette UE
        List<Note> existe = await repositoryFactory.NoteRepository().FindByConditionAsync(n => n.UeId == note.UeId && n.EtudiantId == note.EtudiantId);

        if (existe is { Count: > 0 })
            throw new DuplicateNoteUeException(
                " l'etudiant possede deja une note pour cette ue ."
            );
    }
}