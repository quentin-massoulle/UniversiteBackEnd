using Microsoft.Extensions.Logging;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    protected  readonly UniversiteDbContext Context = context;
    public async Task SaveNotesAsync(List<Note> notes)
    {
        context.Notes.AddRange(notes);
        await context.SaveChangesAsync();
    }
}