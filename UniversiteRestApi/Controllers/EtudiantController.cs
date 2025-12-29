using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.EtudiantUseCases.Delete;
using UniversiteDomain.UseCases.EtudiantUseCases.Get;
using UniversiteDomain.UseCases.EtudiantUseCases.Update;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.UseCases.NoteUseCase.Create;
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.Exceptions.ParcoursExeptions;

namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        // GET: api/<EtudiantController>
        [HttpGet]
        public async Task<ActionResult<List<EtudiantDto>>> Get()
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            List<Etudiant> etudiants = await uc.ExecuteAsync();

            return Ok(etudiants.Select(e => new EtudiantDto().ToDto(e)));
        }

        
        // GET api/<EtudiantApi>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EtudiantDto>> Get(long id)
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            try 
            {
                Etudiant etudiant = await uc.ExecuteAsync(id);
                return Ok(new EtudiantDto().ToDto(etudiant));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // Crée un nouvel étudiant sans parcours
        // POST api/<EtudiantApi>
        [HttpPost]
        public async Task<ActionResult<EtudiantDto>> PostAsync([FromBody] EtudiantDto dto)
        {
            CreateEtudiantUseCase uc=new CreateEtudiantUseCase(repositoryFactory);
            Etudiant etudiant = dto.ToEntity();
            
            try 
            {
                Etudiant created = await uc.ExecuteAsync(etudiant);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, new EtudiantDto().ToDto(created));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT api/<EtudiantController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] EtudiantDto dto)
        {
            UpdateEtudiantUseCase uc = new UpdateEtudiantUseCase(repositoryFactory);
            try
            {
                await uc.ExecuteAsync(id, dto.NumEtud, dto.Nom, dto.Prenom, dto.Email);
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

        // POST api/<EtudiantController>/5/Note
        [HttpPost("{id}/Note")]
        public async Task<ActionResult<NoteDto>> AddNote(long id, [FromBody] NoteDto dto)
        {
            CreateNoteUseCase uc = new CreateNoteUseCase(repositoryFactory);
            Note note = dto.ToEntity();
            note.EtudiantId = id;
            
            try
            {
                Note created = await uc.ExecuteAsync(note);
                return CreatedAtAction("Get", "Note", new { etudiantId = created.EtudiantId, ueId = created.UeId }, new NoteDto().ToDto(created));
            }
            catch (Exception e) when (e is InvalidValeurNoteException || e is InvalidIdUe || e is EtudiantNotFoundException || e is DuplicateNoteUeException)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE api/<EtudiantController>/5
        [HttpDelete("{id}")]
        public async Task Delete(long id)
        {
            DeleteEtudiantUseCase uc=new DeleteEtudiantUseCase(repositoryFactory);
            await uc.ExecuteAsync(id);
        }

        // POST api/Etudiant/5/Parcours
        [HttpPost("{id}/Parcours")]
        public async Task<ActionResult<EtudiantDto>> AddParcours(long id, [FromBody] long parcoursId)
        {
            AddEtudiantDansParcoursUseCase uc = new AddEtudiantDansParcoursUseCase(repositoryFactory);
            try
            {
                // Note: The use case returns the Parcours, but we want to return the updated Etudiant.
                // We need to fetch the student again to get the updated state with the Parcours.
                await uc.ExecuteAsync(parcoursId, id);
                
                GetEtudiantUseCase getUc = new GetEtudiantUseCase(repositoryFactory);
                Etudiant etudiant = await getUc.ExecuteAsync(id);
                
                return Ok(new EtudiantDto().ToDto(etudiant));
            }
            catch (Exception e) when (e is EtudiantNotFoundException || e is ParcoursNotFoundException)
            {
                return NotFound(e.Message);
            }
            catch (DuplicateInscriptionException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
