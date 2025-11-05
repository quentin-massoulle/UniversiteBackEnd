using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IUeRepository ueRepository)
{
    public async Task<Ue> ExecuteAsync(string numeroUe, string intitule)
    {
        var ue = new Ue { NumeroUe = numeroUe, Intitule = intitule };
        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        Ue created = await ueRepository.CreateAsync(ue);
        ueRepository.SaveChangesAsync().Wait();
        return created;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(ueRepository);

        // 🔍 Vérifie si une UE avec le même numéro existe déjà
        List<Ue> existe = await ueRepository.FindByConditionAsync(u => u.NumeroUe.Equals(ue.NumeroUe));

        if (existe is { Count: > 0 })
            throw new DuplicateNumeroUeException(
                ue.NumeroUe + " - ce numéro d'UE est déjà affecté à une autre UE"
            );

        // 🧾 Vérifie que l’intitulé contient au moins 3 caractères
        if (ue.Intitule.Length < 3)
            throw new InvalidIntituleUeException(
                ue.Intitule + " incorrect - L’intitulé d’une UE doit contenir au moins 3 caractères"
            );
    }
}