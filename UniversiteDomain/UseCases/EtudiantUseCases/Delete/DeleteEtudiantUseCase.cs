using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Util;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long etudiantId)
    {
        // 1. Vérification des règles métier avant suppression
        await CheckBusinessRules(etudiantId);

        // 2. Suppression via le repository
        await repositoryFactory.EtudiantRepository().DeleteAsync(etudiantId);

        // 3. Confirmation des changements en base de données
        // Note : await est préférable à .Wait() pour ne pas bloquer le thread
        await repositoryFactory.SaveChangesAsync();
    }

    private async Task CheckBusinessRules(long etudiantId)
    {
        // Validation basique de l'argument
        if (etudiantId <= 0) 
            throw new ArgumentException("L'identifiant de l'étudiant doit être valide");

        // Règle métier : On ne peut pas supprimer un étudiant qui n'existe pas
        // On vérifie si l'étudiant est présent en base
        Etudiant? etudiant = await repositoryFactory.EtudiantRepository().GetByIdAsync(etudiantId);

        if (etudiant == null) 
            throw new EtudiantNotFoundException($"L'étudiant avec l'id {etudiantId} n'existe pas et ne peut donc pas être supprimé.");

        // (Optionnel) Autres règles métier :
        // Par exemple : Si l'étudiant a des notes ou est inscrit à des cours, 
        // on pourrait interdire la suppression ici.
    }
}