using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.ParcoursExeptions;

namespace UniversiteDomain.UseCases.ParcoursUseCase.Update;

public class UpdateParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Parcours> ExecuteAsync(long id, string nomParcours, int anneeFormation)
    {
        var parcours = await repositoryFactory.ParcoursRepository().FindAsync(id);
        if (parcours == null)
        {
            throw new ParcoursNotFoundException($"Le parcours avec l'ID {id} n'existe pas.");
        }

        parcours.NomParcours = nomParcours;
        parcours.AnneeFormation = anneeFormation;

        await repositoryFactory.ParcoursRepository().UpdateAsync(parcours);
        await repositoryFactory.SaveChangesAsync();

        return parcours;
    }
    
    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        return await ExecuteAsync(parcours.Id, parcours.NomParcours, parcours.AnneeFormation);
    }
}
