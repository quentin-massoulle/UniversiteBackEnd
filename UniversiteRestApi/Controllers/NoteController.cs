using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.NoteUseCase.Create;
using UniversiteDomain.UseCases.NoteUseCase.Delete;
using UniversiteDomain.UseCases.NoteUseCase.Get;
using UniversiteDomain.UseCases.NoteUseCase.Update;
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.UeExeptions;


namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController(IRepositoryFactory repositoryFactory)  : ControllerBase
    {
        // GET: api/<NoteController>
        [HttpGet]
        public async Task<ActionResult<List<Note>>> Get()
        {
            GetNoteUseCase uc = new GetNoteUseCase(repositoryFactory);
            List<Note> notes = await uc.ExecuteAsync();
            return Ok(notes);
        }

        // GET api/<NoteController>/5/10
        [HttpGet("{etudiantId}/{ueId}")]
        public async Task<ActionResult<Note>> Get(long etudiantId, long ueId)
        {
            GetNoteUseCase uc = new GetNoteUseCase(repositoryFactory);
            Note? note = await uc.ExecuteAsync(etudiantId, ueId);
            if (note == null) return NotFound();
            return Ok(note);
        }

        // POST api/<NoteController>
        [HttpPost]
        public async Task<ActionResult<Note>> PostAsync([FromBody] Note note)
        {
            CreateNoteUseCase uc = new CreateNoteUseCase(repositoryFactory);
            try
            {
                Note created = await uc.ExecuteAsync(note);
                return CreatedAtAction(nameof(Get), new { etudiantId = created.EtudiantId, ueId = created.UeId }, created);
            }
            catch (Exception e) when (e is InvalidValeurNoteException || e is InvalidIdUe || e is EtudiantNotFoundException || e is DuplicateNoteUeException)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT api/<NoteController>/5/10
        [HttpPut("{etudiantId}/{ueId}")]
        public async Task<ActionResult<Note>> Put(long etudiantId, long ueId, [FromBody] Note note)
        {
            UpdateNoteUseCase uc = new UpdateNoteUseCase(repositoryFactory);
            try
            {
                await uc.ExecuteAsync(etudiantId, ueId, note.Valeur);
                return NoContent();
            }
            catch (NoteNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidValeurNoteException e)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE api/<NoteController>/5/10
        [HttpDelete("{etudiantId}/{ueId}")]
        public async Task<ActionResult> Delete(long etudiantId, long ueId)
        {
            DeleteNoteUseCase uc = new DeleteNoteUseCase(repositoryFactory);
            await uc.ExecuteAsync(etudiantId, ueId);
            return NoContent();
        }
    }
}
