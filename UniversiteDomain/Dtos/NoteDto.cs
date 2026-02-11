using UniversiteDomain.Entites;

namespace UniversiteDomain.Dtos;

public class NoteDto
{
    public float Valeur { get; set; }
    public long EtudiantId { get; set; }
    public long UeId { get; set; }

    public NoteDto ToDto(Note note)
    {
        Valeur = note.Valeur;
        EtudiantId = note.EtudiantId;
        UeId = note.UeId;
        return this;
    }

    public Note ToEntity()
    {
        return new Note
        {
            Valeur = this.Valeur,
            EtudiantId = this.EtudiantId,
            UeId = this.UeId
        };
    }
}
