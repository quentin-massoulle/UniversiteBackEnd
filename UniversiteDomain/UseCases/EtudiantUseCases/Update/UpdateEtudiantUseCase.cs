using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Util;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Update;

public class UpdateEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Etudiant> ExecuteAsync(long id, string numEtud, string nom, string prenom, string email)
    {
        var etudiant = await repositoryFactory.EtudiantRepository().FindAsync(id);
        if (etudiant == null)
        {
            throw new EtudiantNotFoundException($"L'étudiant avec l'ID {id} n'existe pas.");
        }

        etudiant.NumEtud = numEtud;
        etudiant.Nom = nom;
        etudiant.Prenom = prenom;
        etudiant.Email = email;
        
        await CheckBusinessRules(etudiant);

        await repositoryFactory.EtudiantRepository().UpdateAsync(etudiant);
        await repositoryFactory.SaveChangesAsync();

        return etudiant;
    }
    
    public async Task<Etudiant> ExecuteAsync(Etudiant etudiant)
    {
        return await ExecuteAsync(etudiant.Id, etudiant.NumEtud, etudiant.Nom, etudiant.Prenom, etudiant.Email);
    }
    
    private async Task CheckBusinessRules(Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(etudiant);
        ArgumentNullException.ThrowIfNull(etudiant.NumEtud);
        ArgumentNullException.ThrowIfNull(etudiant.Email);
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        
        // On recherche un étudiant avec le même numéro étudiant (autre que lui-même)
        List<Etudiant> existe = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e=>e.NumEtud.Equals(etudiant.NumEtud) && e.Id != etudiant.Id);

        // Si un étudiant avec le même numéro étudiant existe déjà, on lève une exception personnalisée
        if (existe is {Count:>0}) throw new DuplicateNumEtudException(etudiant.NumEtud+ " - ce numéro d'étudiant est déjà affecté à un étudiant");
        
        // Vérification du format du mail
        if (!CheckEmail.IsValidEmail(etudiant.Email)) throw new InvalidEmailException(etudiant.Email + " - Email mal formé");
        
        // On vérifie si l'email est déjà utilisé (autre que lui-même)
        existe = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e=>e.Email.Equals(etudiant.Email) && e.Id != etudiant.Id);
        // Une autre façon de tester la vacuité de la liste
        if (existe is {Count:>0}) throw new DuplicateEmailException(etudiant.Email +" est déjà affecté à un étudiant");
        // Le métier définit que les nom doite contenir plus de 3 lettres
        if (etudiant.Nom.Length < 3) throw new InvalidNomEtudiantException(etudiant.Nom +" incorrect - Le nom d'un étudiant doit contenir plus de 3 caractères");
    }
}
