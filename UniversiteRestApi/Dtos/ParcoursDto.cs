using UniversiteDomain.Entites;

namespace UniversiteDomain.Dtos;

public class ParcoursDto
{
    public long Id { get; set; }
    public string NomParcours { get; set; } = string.Empty;
    public int AnneeFormation { get; set; }
    public List<EtudiantDto> Inscrits { get; set; } = new();
    public List<UeDto> UesEnseignees { get; set; } = new();

    public ParcoursDto ToDto(Parcours parcours)
    {
        Id = parcours.Id;
        NomParcours = parcours.NomParcours;
        AnneeFormation = parcours.AnneeFormation;
        
        if (parcours.Inscrits != null)
        {
            Inscrits = parcours.Inscrits.Select(e => new EtudiantDto().ToDto(e)).ToList();
        }
        
        if (parcours.UesEnseignees != null)
        {
            UesEnseignees = parcours.UesEnseignees.Select(u => new UeDto().ToDto(u)).ToList();
        }
        
        return this;
    }

    public Parcours ToEntity()
    {
        return new Parcours
        {
            Id = this.Id,
            NomParcours = this.NomParcours,
            AnneeFormation = this.AnneeFormation
        };
    }
}
