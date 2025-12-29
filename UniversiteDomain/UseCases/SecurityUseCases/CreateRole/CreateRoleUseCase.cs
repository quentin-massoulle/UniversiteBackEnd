using UniversiteDomain.DataAdapters;

namespace UniversiteDomain.UseCases.SecurityUseCases.CreateRole;

public class CreateRoleUseCase
{
    private readonly IUniversiteRoleRepository _roleRepository;

    public CreateRoleUseCase(IUniversiteRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> ExecuteAsync(string roleName)
    {
        return await _roleRepository.AddRoleAsync(roleName);
    }
}
