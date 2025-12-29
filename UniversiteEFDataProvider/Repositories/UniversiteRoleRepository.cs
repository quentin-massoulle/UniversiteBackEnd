using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entites;

namespace UniversiteEFDataProvider.Repositories;

public class UniversiteRoleRepository : IUniversiteRoleRepository
{
    private readonly RoleManager<UniversiteRole> _roleManager;
    private readonly UniversiteDbContext _context;

    public UniversiteRoleRepository(RoleManager<UniversiteRole> roleManager, UniversiteDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<bool> AddRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new UniversiteRole { Name = roleName });
            return result.Succeeded;
        }
        return false;
    }

    public async Task<IUniversiteRole> CreateAsync(IUniversiteRole entity)
    {
        throw new NotImplementedException("Use AddRoleAsync instead");
    }

    public async Task UpdateAsync(IUniversiteRole entity)
    {
        await _roleManager.UpdateAsync((UniversiteRole)entity);
    }

    public async Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(IUniversiteRole entity)
    {
        await _roleManager.DeleteAsync((UniversiteRole)entity);
    }

    public async Task<IUniversiteRole?> FindAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IUniversiteRole?> FindAsync(params object[] keyValues)
    {
        if (keyValues.Length > 0 && keyValues[0] is string id)
        {
             return await _roleManager.FindByIdAsync(id);
        }
        return null;
    }

    public async Task<List<IUniversiteRole>> FindByConditionAsync(Expression<Func<IUniversiteRole, bool>> condition)
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Cast<IUniversiteRole>().AsQueryable().Where(condition).ToList();
    }

    public async Task<List<IUniversiteRole>> FindAllAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Cast<IUniversiteRole>().ToList();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
