using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;


namespace UniversiteDomain.UseCases.UeUseCase.Get;
public class GetUeUseCase(IUeRepository ueRepository)
{
    public async Task<Ue> ExecuteAsync(long ueId)
    {
        // 1. Validation d'entrée minimale
        if (ueId <= 0)
        {
            throw new ArgumentException("L'identifiant de l'UE doit être valide.", nameof(ueId));
        }

        // 2. Appel du Repository pour la récupération
        Ue? ue = await ueRepository.GetByIdAsync(ueId); 

        // 3. Vérification du résultat (Règle d'Application)
        if (ue == null)
        {
            // Lève une exception si l'UE n'existe pas (règle métier d'existence)
            throw new UeNotFoundException($"L'Unité d'Enseignement avec l'ID {ueId} n'a pas été trouvée.");
        }

        // 4. Retourne l'entité
        return ue;
    }
}