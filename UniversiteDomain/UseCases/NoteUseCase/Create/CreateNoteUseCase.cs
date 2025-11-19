using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCase.Get;
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.NoteUseCase.Create;

public class CreateNoteUseCase(INoteRepository noteRepository)
{
    private readonly IUeRepository _ueRepository;
    public async Task<Note> ExecuteAsync(float Valeur,long EtudiantId ,long UeId )
    {
        var note = new Note {Valeur= Valeur , EtudiantId = EtudiantId, UeId = UeId};
        return await ExecuteAsync(note);
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        await CheckBusinessRules(note);
        Note created = await noteRepository.CreateAsync(note);
        noteRepository.SaveChangesAsync().Wait();
        return created;
    }

    private async Task CheckBusinessRules(Note note)
    {
        
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(note.UeId);
        ArgumentNullException.ThrowIfNull(note.EtudiantId);
        ArgumentNullException.ThrowIfNull(note.Valeur);
        ArgumentNullException.ThrowIfNull(noteRepository);
        
        
        //verifie la note est comprise entre 0 et 20 
        if (note.Valeur>=0 && note.Valeur<= 20 )
            throw new InvalidValeurNoteException(
                note.Valeur + " la note doit etre comprise entre 0 et 20 "
            );
        // Un étudiant ne peut avoir une note que dans une Ue du parcours dans lequel il est inscrit
        // doit verifier si l'eu existe 
        Ue? ueExistante = await _ueRepository.GetByIdAsync(note.UeId);
        if (ueExistante != null)
            throw new InvalidIdUe(
                note.Valeur + " la note doit etre comprise entre 0 et 20 "
            );
        
        
        // doit verifier si l'etudiant est inscrit dans le parcours 
        List<Note> existe = await noteRepository.FindByConditionAsync(n => n.UeId.Equals(note.UeId) );

        if (existe is { Count: > 0 })
            throw new DuplicateNoteUeException(
                note.Valeur + " la note doit etre comprise entre 0 et 20 "
            );
        //
        existe = await noteRepository.FindByConditionAsync(n => n.UeId.Equals(note.Etudiant.ParcoursSuivi.Id) );

        if (existe is { Count: > 0 })
            throw new InvalidUeEtudiantExeption(
                
            );
    }
}