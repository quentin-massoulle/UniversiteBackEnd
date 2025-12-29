using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCase.Update;

public class AddParcoursToUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Ue> ExecuteAsync(long idUe, long idParcours)
    {
        try
        {
            return await repositoryFactory.UeRepository().AddParcoursAsync(idUe, idParcours);
        }
        catch (NullReferenceException)
        {
            throw new UeNotFoundException($"L'UE {idUe} ou le Parcours {idParcours} n'existe pas.");
        }
    }
    
    public async Task<Ue> ExecuteAsync(long idUe, long[] idParcours)
    {
        try
        {
            return await repositoryFactory.UeRepository().AddParcoursAsync(idUe, idParcours);
        }
        catch (NullReferenceException)
        {
            throw new UeNotFoundException($"L'UE {idUe} n'existe pas.");
        }
    }
}
