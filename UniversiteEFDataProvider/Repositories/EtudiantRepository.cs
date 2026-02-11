using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;

namespace UniversiteEFDataProvider.Repositories;

public class EtudiantRepository(UniversiteDbContext context) : Repository<Etudiant>(context), IEtudiantRepository
{
    protected  readonly UniversiteDbContext Context = context;
    public async Task AffecterParcoursAsync(long idEtudiant, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        e.ParcoursSuivi = p;
        await Context.SaveChangesAsync();
    }
    public async Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants.Include(e => e.NotesObtenues).ThenInclude(n=>n.Ue).FirstOrDefaultAsync(e => e.Id == idEtudiant);
    }
    
    public async Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours)
    {
        await AffecterParcoursAsync(etudiant.Id, parcours.Id); 
    }
    public async Task<Etudiant?> GetByIdAsync(long id)
    {
        return await context.Etudiants
            .Include(e => e.ParcoursSuivi)
            .Include(e => e.NotesObtenues)
            .FirstOrDefaultAsync(e => e.Id == id); 
    }

    public async Task<List<Etudiant>> GetAllAsync()
    {
        return await context.Etudiants
            .Include(e => e.ParcoursSuivi)
            .Include(e => e.NotesObtenues)
            .ToListAsync();
    }
    
}