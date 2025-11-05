using UniversiteDomain.Exceptions.ParcoursExeptions;

namespace UniversiteDomain.UseCases.ParcoursUseCase;
using UniversiteDomain.Entites;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;

public class CreateParcoursUseCase
{
    private readonly IRepositoryFactory FactoryRepository;

    public CreateParcoursUseCase(IRepositoryFactory FactoryRepository)
    {
        this.FactoryRepository = FactoryRepository;
    }

    public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours
        {
            NomParcours = nomParcours,
            AnneeFormation = anneeFormation
        };
        return await ExecuteAsync(parcours);
    }

    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        Parcours created = await FactoryRepository.ParcoursRepository().CreateAsync(parcours);
        FactoryRepository.SaveChangesAsync().Wait();
        return created;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        ArgumentNullException.ThrowIfNull(FactoryRepository);

        // Vérifier qu'un parcours avec le même nom n'existe pas déjà
        List<Parcours> existe = await FactoryRepository.ParcoursRepository().FindByConditionAsync(
            p => p.NomParcours.Equals(parcours.NomParcours)
        );

        if (existe is { Count: > 0 })
            throw new DuplicateFormations(
                parcours.NomParcours + " - ce nom de parcours est déjà utilisé"
            );

        // Vérifier que le nom du parcours contient au moins 3 caractères
        if (parcours.NomParcours.Length < 3)
            throw new InvalidNomParcoursException(
                parcours.NomParcours + " incorrect - Le nom d'un parcours doit contenir au moins 3 caractères"
            );

        // Vérifier que l’année de formation est valide (par exemple entre 1 et 5 pour un master)
        if (parcours.AnneeFormation < 1 || parcours.AnneeFormation > 5)
            throw new InvalidAnneeFormationException(
                "Année de formation " + parcours.AnneeFormation + " invalide - elle doit être comprise entre 1 et 5"
            );
    }
}
