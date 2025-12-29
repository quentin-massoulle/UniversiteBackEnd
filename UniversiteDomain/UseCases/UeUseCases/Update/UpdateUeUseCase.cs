using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCase.Update;

public class UpdateUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Ue> ExecuteAsync(long id, string numeroUe, string intitule)
    {
        var ue = await repositoryFactory.UeRepository().FindAsync(id);
        if (ue == null)
        {
            throw new UeNotFoundException($"L'UE avec l'ID {id} n'existe pas.");
        }

        ue.NumeroUe = numeroUe;
        ue.Intitule = intitule;

        await repositoryFactory.UeRepository().UpdateAsync(ue);
        await repositoryFactory.SaveChangesAsync();

        return ue;
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        return await ExecuteAsync(ue.Id, ue.NumeroUe, ue.Intitule);
    }
}
