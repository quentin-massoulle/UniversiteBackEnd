using System.Linq.Expressions;
using UniversiteDomain.Entites;
 
namespace UniversiteDomain.DataAdapters;
 
public interface IEtudiantRepository : IRepository<Etudiant>
{
    Task<Etudiant?> GetByIdAsync(long id);
    public Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant);
    
    Task<List<Etudiant>> GetAllAsync();
    
}