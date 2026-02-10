using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entites;

namespace UniversiteEFDataProvider.Entities;

public class UniversiteUser:IdentityUser, IUniversiteUser {
    [PersonalData]
    public Etudiant? Etudiant { get; set; }
    [PersonalData]
    public long? EtudiantId { get; set; }
}