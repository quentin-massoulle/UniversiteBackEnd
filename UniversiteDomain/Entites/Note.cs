using System.Text.Json.Serialization;

namespace UniversiteDomain.Entites;

public class Note
{
    public float Valeur { get; set; }
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    
    [JsonIgnore]
    public Etudiant? Etudiant { get; set; }
    [JsonIgnore]
    public Ue? Ue { get; set; }
    
    public override string ToString()
    {
        return "ID etudiant" +" : "+EtudiantId+" Ue ID ; " + UeId + " valeur ; " +  Valeur;
    }
}