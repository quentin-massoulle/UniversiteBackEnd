using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Get;

// J'ai renommé la classe au singulier "UseCase" par convention
public class GetEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Etudiant> ExecuteAsync(long etudiantId)
    {
        if (etudiantId <= 0)
        {
            throw new ArgumentException("L'identifiant de l'étudiant doit être valide (supérieur à 0).", nameof(etudiantId));
        }
        Etudiant? etudiant = await repositoryFactory.EtudiantRepository().GetByIdAsync(etudiantId); 
        if (etudiant == null)
        {
            throw new EtudiantNotFoundException($"L'étudiant avec l'ID {etudiantId} n'a pas été trouvé.");
        }
        return etudiant;
    }

    public async Task<List<Etudiant>> ExecuteAsync()
    {
        // On appelle la méthode du repository qui renvoie tout (souvent appelée GetAllAsync ou FindAllAsync)
        List<Etudiant> etudiants = await repositoryFactory.EtudiantRepository().GetAllAsync();
    
        // Optionnel : Vous pouvez lever une exception si la liste est vide, 
        // mais conventionnellement on renvoie juste une liste vide [].
        return etudiants;
    }
}