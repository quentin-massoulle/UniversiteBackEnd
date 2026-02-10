namespace UniversiteDomain.DataAdapters.DataAdaptersFactory;

public interface IRepositoryFactory
{
    IParcoursRepository ParcoursRepository();
    IEtudiantRepository EtudiantRepository();
    
    IUniversiteRoleRepository UniversiteRoleRepository();
    IUniversiteUserRepository UniversiteUserRepository();
    
    IUeRepository UeRepository();
    
    INoteRepository NoteRepository();
    
    Task EnsureDeletedAsync();
    Task EnsureCreatedAsync();
    Task SaveChangesAsync();
}