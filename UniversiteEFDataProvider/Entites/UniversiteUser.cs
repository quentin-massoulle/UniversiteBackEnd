using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entites;

namespace UniversiteEFDataProvider.Entites;

public class UniversiteUser : IdentityUser, IUniversiteUser
{
    public long? EtudiantId { get; set; }
    public Etudiant? Etudiant { get; set; }
}
