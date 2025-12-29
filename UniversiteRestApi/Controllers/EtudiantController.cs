using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.EtudiantUseCases.Delete;
using UniversiteDomain.UseCases.EtudiantUseCases.Get;
using UniversiteDomain.UseCases.EtudiantUseCases.Update;
using UniversiteDomain.Exceptions.EtudiantExceptions;

namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        // GET: api/<EtudiantController>
        [HttpGet]
        public async Task<ActionResult<Etudiant>> Get()
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            List<Etudiant> etudiants = await uc.ExecuteAsync();

            
            return Ok(etudiants);
        }

        
        // GET api/<EtudiantApi>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Etudiant>> Get(long id)
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            Etudiant etudiant = await uc.ExecuteAsync(id);

            // On retourne le code HTTP 200 (OK) avec les données de l'étudiant
            return Ok(etudiant);

        }

        // Crée un nouvel étudiant sans parcours
        // POST api/<EtudiantApi>
        [HttpPost]
        public async Task PostAsync([FromBody] Etudiant etudiant)
        {
            CreateEtudiantUseCase uc=new CreateEtudiantUseCase(repositoryFactory);
            await uc.ExecuteAsync(etudiant);
        }

        // PUT api/<EtudiantController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Etudiant>> Put(long id, [FromBody] Etudiant etudiant)
        {
            UpdateEtudiantUseCase uc = new UpdateEtudiantUseCase(repositoryFactory);
            try
            {
                await uc.ExecuteAsync(id, etudiant.NumEtud, etudiant.Nom, etudiant.Prenom, etudiant.Email);
                return NoContent();
            }
            catch (EtudiantNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e) when (e is DuplicateNumEtudException || e is InvalidEmailException || e is DuplicateEmailException || e is InvalidNomEtudiantException)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE api/<EtudiantController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            DeleteEtudiantUseCase uc=new DeleteEtudiantUseCase(repositoryFactory);
            await uc.ExecuteAsync(id);
        }
    }
}
