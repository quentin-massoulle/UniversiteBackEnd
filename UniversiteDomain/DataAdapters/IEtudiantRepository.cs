using System.Linq.Expressions;
using UniversiteDomain.Entites;
 
namespace UniversiteDomain.DataAdapters;
 
public interface IEtudiantRepository : IRepository<Etudiant>
{
    Task<Etudiant?> GetByIdAsync(long id);
    
    Task<List<Etudiant>> GetAllAsync();
    
    
    Task  DeleteAsync(long id);
    
}