using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;

namespace UniversiteDomain.UseCases.SecurityUseCases.CreateUser;

public class CreateUserUseCase
{
    private readonly IUniversiteUserRepository _userRepository;

    public CreateUserUseCase(IUniversiteUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IUniversiteUser> ExecuteAsync(string login, string email, string password, string role, long? etudiantId = null)
    {
        var user = await _userRepository.AddUserAsync(login, email, password, etudiantId);
        if (user != null && !string.IsNullOrEmpty(role))
        {
            await _userRepository.AddToRoleAsync(user, role);
        }
        return user;
    }
}
