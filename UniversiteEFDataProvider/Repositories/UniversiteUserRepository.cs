using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entites;

namespace UniversiteEFDataProvider.Repositories;

public class UniversiteUserRepository : IUniversiteUserRepository
{
    private readonly UserManager<UniversiteUser> _userManager;
    private readonly UniversiteDbContext _context;

    public UniversiteUserRepository(UserManager<UniversiteUser> userManager, UniversiteDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IUniversiteUser> AddUserAsync(string login, string email, string password, long? etudiantId = null)
    {
        var user = new UniversiteUser
        {
            UserName = login,
            Email = email,
            EtudiantId = etudiantId
        };
        
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create user: {errors}");
        }
        return user;
    }

    public async Task<bool> AddToRoleAsync(IUniversiteUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync((UniversiteUser)user, role);
        return result.Succeeded;
    }

    public async Task<IUniversiteUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IUniversiteUser> CreateAsync(IUniversiteUser entity)
    {
        throw new NotImplementedException("Use AddUserAsync instead");
    }

    public async Task UpdateAsync(IUniversiteUser entity)
    {
        await _userManager.UpdateAsync((UniversiteUser)entity);
    }

    public async Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(IUniversiteUser entity)
    {
        await _userManager.DeleteAsync((UniversiteUser)entity);
    }

    public async Task<IUniversiteUser?> FindAsync(long id)
    {
         throw new NotImplementedException();
    }

    public async Task<IUniversiteUser?> FindAsync(params object[] keyValues)
    {
         if (keyValues.Length > 0 && keyValues[0] is string id)
        {
             return await _userManager.FindByIdAsync(id);
        }
        return null;
    }

    public async Task<List<IUniversiteUser>> FindByConditionAsync(Expression<Func<IUniversiteUser, bool>> condition)
    {
        var users = await _userManager.Users.ToListAsync();
        return users.Cast<IUniversiteUser>().AsQueryable().Where(condition).ToList();
    }

    public async Task<List<IUniversiteUser>> FindAllAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        return users.Cast<IUniversiteUser>().ToList();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
