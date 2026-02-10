namespace UniversiteDomain.Entites;

public interface IUniversiteUser
{
    string Id { get; set; }
    string UserName { get; set; }
    string Email { get; set; }
    long ? EtudiantId { get; set; }
    Etudiant? Etudiant { get; set; }
}