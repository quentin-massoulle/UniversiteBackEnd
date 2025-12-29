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
using UniversiteDomain.Dtos;


namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController(IRepositoryFactory repositoryFactory)  : ControllerBase
    {
        // GET: api/<NoteController>
        [HttpGet]
        public async Task<ActionResult<List<NoteDto>>> Get()
        {
            GetNoteUseCase uc = new GetNoteUseCase(repositoryFactory);
            List<Note> notes = await uc.ExecuteAsync();
            return Ok(notes.Select(n => new NoteDto().ToDto(n)));
        }

        // GET api/<NoteController>/5/10
        [HttpGet("{etudiantId}/{ueId}")]
        public async Task<ActionResult<NoteDto>> Get(long etudiantId, long ueId)
        {
            GetNoteUseCase uc = new GetNoteUseCase(repositoryFactory);
            Note? note = await uc.ExecuteAsync(etudiantId, ueId);
            if (note == null) return NotFound();
            return Ok(new NoteDto().ToDto(note));
        }

        // POST api/<NoteController>
        [HttpPost]
        public async Task<ActionResult<NoteDto>> PostAsync([FromBody] NoteDto dto)
        {
            CreateNoteUseCase uc = new CreateNoteUseCase(repositoryFactory);
            Note note = dto.ToEntity();
            try
            {
                Note created = await uc.ExecuteAsync(note);
                return CreatedAtAction(nameof(Get), new { etudiantId = created.EtudiantId, ueId = created.UeId }, new NoteDto().ToDto(created));
            }
            catch (Exception e) when (e is InvalidValeurNoteException || e is InvalidIdUe || e is EtudiantNotFoundException || e is DuplicateNoteUeException)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT api/<NoteController>/5/10
        [HttpPut("{etudiantId}/{ueId}")]
        public async Task<ActionResult> Put(long etudiantId, long ueId, [FromBody] NoteDto dto)
        {
            UpdateNoteUseCase uc = new UpdateNoteUseCase(repositoryFactory);
            try
            {
                await uc.ExecuteAsync(etudiantId, ueId, dto.Valeur);
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
