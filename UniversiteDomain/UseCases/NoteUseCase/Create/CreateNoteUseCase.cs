using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.UseCases.UeUseCase.Get;
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.NoteUseCase.Create;

public class CreateNoteUseCase(
    INoteRepository noteRepository,
    IUeRepository ueRepository,
    IEtudiantRepository etudiantRepository)
{
    // Ils sont maintenant disponibles directement pour la classe
    private readonly INoteRepository _noteRepository = noteRepository;
    private readonly IUeRepository _ueRepository = ueRepository;
    private readonly IEtudiantRepository _etudiantRepository = etudiantRepository;
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
        if (note.Valeur < 0 && note.Valeur > 20)
            throw new InvalidValeurNoteException(
                note.Valeur + " la note doit etre comprise entre 0 et 20 "
            );
        // doit verifier si l'eu existe 
        Ue? ueExistante = await _ueRepository.GetByIdAsync(note.UeId);
        if (ueExistante == null)
            throw new InvalidIdUe(
                note.UeId + " l'ue n'existe pas "
            );
        
        // doit verifier si l'etudiant est inscrit dans le parcours 
        Etudiant? etudiantExiste = await _etudiantRepository.GetByIdAsync(note.EtudiantId);
        if (etudiantExiste == null)
            throw new EtudiantNotFoundException(
                note. EtudiantId+ " l'etudiant n'existe pas "
            );
        
        long idParcoursEtudiant = etudiantExiste.ParcoursSuivi.Id;
        
        bool estEligible = ueExistante.EnseigneeDans != null && 
                           ueExistante.EnseigneeDans.Any(p => p.Id == idParcoursEtudiant);

        if (!estEligible)
        {
            throw new InvalidUeEtudiantExeption(
                $"L'UE {note.UeId} n'est pas enseignée dans le parcours {idParcoursEtudiant} de l'étudiant."
            );
        }
        List<Note> existe = await noteRepository.FindByConditionAsync(n => n.UeId.Equals(note.UeId) );

        if (existe is { Count: > 0 })
            throw new DuplicateNoteUeException(
                " l'etudiant possede deja une note pour cette eu ."
            );
        //
        existe = await noteRepository.FindByConditionAsync(n => n.UeId.Equals(note.Etudiant.ParcoursSuivi.Id) );

        if (existe is { Count: > 0 })
            throw new InvalidUeEtudiantExeption(
                
            );
    }
}