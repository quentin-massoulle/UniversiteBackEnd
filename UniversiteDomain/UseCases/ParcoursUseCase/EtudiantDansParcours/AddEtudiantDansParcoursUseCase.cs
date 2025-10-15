using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.ParcoursExeptions;
using UniversiteDomain.Exceptions.EtudiantExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;

public class AddEtudiantDansParcoursUseCase(IEtudiantRepository etudiantRepository, IParcoursRepository parcoursRepository)
{
    // Rajout d'un étudiant dans un parcours
      public async Task<Parcours> ExecuteAsync(Parcours parcours, Etudiant etudiant)
      {
          ArgumentNullException.ThrowIfNull(parcours);
          ArgumentNullException.ThrowIfNull(etudiant);
          return await ExecuteAsync(parcours.Id, etudiant.Id); 
      }  
      public async Task<Parcours> ExecuteAsync(long idParcours, long idEtudiant)
      {
          await CheckBusinessRules(idParcours, idEtudiant); 
          return await parcoursRepository.AddEtudiantAsync(idParcours, idEtudiant);
      }

      // Rajout de plusieurs étudiants dans un parcours
      public async Task<Parcours> ExecuteAsync(Parcours parcours, List<Etudiant> etudiants)
      {
          long[] idEtudiants = etudiants.Select(x => x.Id).ToArray();
          return await ExecuteAsync(parcours.Id, idEtudiants); 
      }  
      public async Task<Parcours> ExecuteAsync(long idParcours, long [] idEtudiants)
      {
        foreach(var id in idEtudiants) await CheckBusinessRules(idParcours, id);
        return await parcoursRepository.AddEtudiantAsync(idParcours, idEtudiants);
      }   

    private async Task CheckBusinessRules(long idParcours, long idEtudiant)
    {
				// Vérification des paramètres
        ArgumentNullException.ThrowIfNull(idParcours);
        ArgumentNullException.ThrowIfNull(idEtudiant);
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idParcours);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);
        
        // Vérifions tout d'abord que nous sommes bien connectés aux datasources
        ArgumentNullException.ThrowIfNull(etudiantRepository);
        ArgumentNullException.ThrowIfNull(parcoursRepository);
        
        // On recherche l'étudiant
        List<Etudiant> etudiant = await etudiantRepository.FindByConditionAsync(e=>e.Id.Equals(idEtudiant));;
        if (etudiant is { Count: 0 }) throw new EtudiantNotFoundException(idEtudiant.ToString());
        // On recherche le parcours
        List<Parcours> parcours = await parcoursRepository.FindByConditionAsync(p=>p.Id.Equals(idParcours));;
        if (parcours is { Count: 0 }) throw new ParcoursNotFoundException(idParcours.ToString());
        
        // On vérifie que l'étudiant n'est pas déjà dans le parcours
        List<Etudiant> inscrit = await etudiantRepository.FindByConditionAsync(e=>e.Id.Equals(idEtudiant) && e.ParcoursSuivi.Id.Equals(idParcours));
        if (inscrit is { Count: > 0 }) throw new DuplicateInscriptionException(idEtudiant+" est déjà inscrit dans le parcours dans le parcours : "+idParcours);      
    }
}