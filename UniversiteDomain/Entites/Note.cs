namespace UniversiteDomain.Entites;

public class Note
{
    public float Valeur { get; set; }
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    
    public Etudiant Etudiant { get; set; } = new();
    public Ue Ue { get; set; } = new();
}