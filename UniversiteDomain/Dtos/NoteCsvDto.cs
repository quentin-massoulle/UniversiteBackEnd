namespace UniversiteDomain.Dtos;

public class NoteCsvDto
{
    public string NumeroUe { get; set; } = string.Empty;
    public string Intitule { get; set; } = string.Empty;
    public string NumEtud { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public float? Note { get; set; }
}
