using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversiteDomain.Entites;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository (UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    protected  readonly UniversiteDbContext Context = context;

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        if (e != null)
        {
            p.Inscrits.Add(e);
        }
        await Context.SaveChangesAsync();
        return p;
    }
    
    public async Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        return await  AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        
        foreach (var etudiant in etudiants)
        {
            await AddEtudiantAsync(parcours.Id, etudiant.Id);
        }
        Parcours p = (await Context.Parcours.FindAsync(parcours.Id))!;
    
        return p;
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        foreach (var id in idEtudiants)
        {
            await AddEtudiantAsync(idParcours, id);
        }
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
    
        return p;
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        if (ue != null)
        {
            p.UesEnseignees.Add(ue);
        }
        await Context.SaveChangesAsync();
        return p;
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUe)
    {
        foreach (var id in idUe)
        {
            await AddUeAsync(idParcours, id);
        }
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
    
        return p;
    }
    
}