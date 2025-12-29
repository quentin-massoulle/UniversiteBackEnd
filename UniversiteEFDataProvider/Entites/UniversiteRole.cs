using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entites;

namespace UniversiteEFDataProvider.Entites;

public class UniversiteRole : IdentityRole, IUniversiteRole
{
}
