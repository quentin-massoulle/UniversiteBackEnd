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
        this.Id = parcours.Id;
        this.NomParcours = parcours.NomParcours;
        this.AnneeFormation = parcours.AnneeFormation;
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
