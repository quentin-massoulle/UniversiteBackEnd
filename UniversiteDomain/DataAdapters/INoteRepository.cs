using UniversiteDomain.Entites;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task SaveNotesAsync(List<Note> notes);
}