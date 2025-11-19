using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;


namespace UniversiteDomain.UseCases.EtudiantUseCases.Get;

public class GetEtudiantUseCases (IEtudiantRepository etudiantRepository)
{
    
    public async Task<Etudiant> ExecuteAsync(long etudiantId)
    {
        // 1. Validation d'entrée minimale
        if (etudiantId <= 0)
        {
            throw new ArgumentException("L'identifiant de l'etudiant doit être valide.", nameof(etudiantId));
        }

        // 2. Appel du Repository pour la récupération
        Etudiant? etudiant = await etudiantRepository.GetByIdAsync(etudiantId); 

        // 3. Vérification du résultat (Règle d'Application)
        if (etudiant == null)
        {
            // Lève une exception si l'etudiant n'existe pas (règle métier d'existence)
            throw new EtudiantNotFoundException($"L'Unité d'Enseignement avec l'ID {etudiantId} n'a pas été trouvée.");
        }

        // 4. Retourne l'entité
        return etudiant;
    }
    
}