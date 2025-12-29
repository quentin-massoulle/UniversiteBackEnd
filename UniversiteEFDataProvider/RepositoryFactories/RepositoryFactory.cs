using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteEFDataProvider.Entites;
using UniversiteEFDataProvider.Repositories;

namespace UniversiteEFDataProvider.RepositoryFactories;

public class RepositoryFactory (UniversiteDbContext context, UserManager<UniversiteUser> userManager, RoleManager<UniversiteRole> roleManager): IRepositoryFactory
{
    private IParcoursRepository? _parcours;
    private IEtudiantRepository? _etudiants;
    private IUeRepository? _ues;
    private INoteRepository? _notes;
    private IUniversiteRoleRepository? _roles;
    private IUniversiteUserRepository? _users;
    
    public IParcoursRepository ParcoursRepository()
    {
        if (_parcours == null)
        {
            _parcours = new ParcoursRepository(context ?? throw new InvalidOperationException());
        }
        return _parcours;
    }

    public IEtudiantRepository EtudiantRepository()
    {
        if (_etudiants == null)
        {
            _etudiants = new EtudiantRepository(context ?? throw new InvalidOperationException());
        }
        return _etudiants;
    }

    public IUeRepository UeRepository()
    {
        if (_ues == null)
        {
            _ues = new UeRepository(context ?? throw new InvalidOperationException());
        }
        return _ues;
    }

    public INoteRepository NoteRepository()
    {
        if (_notes == null)
        {
            _notes = new NoteRepository(context ?? throw new InvalidOperationException());
        }
        return _notes;
    }

    public IUniversiteRoleRepository UniversiteRoleRepository()
    {
        if (_roles == null)
        {
            _roles = new UniversiteRoleRepository(roleManager, context);
        }
        return _roles;
    }

    public IUniversiteUserRepository UniversiteUserRepository()
    {
        if (_users == null)
        {
            _users = new UniversiteUserRepository(userManager, context);
        }
        return _users;
    }
       
    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
    public async Task EnsureCreatedAsync()
    {
        await context.Database.EnsureCreatedAsync();
    }
    public async Task EnsureDeletedAsync()
    {
        await context.Database.EnsureDeletedAsync();
    }
}