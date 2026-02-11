using UniversiteDomain.Entites;

namespace UniversiteDomain.Dtos;

public class UeDto
{
    public long Id { get; set; }
    public string NumeroUe { get; set; } = string.Empty;
    public string Intitule { get; set; } = string.Empty;

    public UeDto ToDto(Ue ue)
    {
        if (ue == null) return null;
        Id = ue.Id;
        NumeroUe = ue.NumeroUe;
        Intitule = ue.Intitule;
        return this;
    }

    public Ue ToEntity()
    {
        return new Ue
        {
            Id = this.Id,
            NumeroUe = this.NumeroUe,
            Intitule = this.Intitule
        };
    }
}
