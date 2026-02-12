using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCase.Delete;
using UniversiteDomain.UseCases.UeUseCase.Get;
using UniversiteDomain.UseCases.UeUseCase.Update;
using UniversiteDomain.UseCases.UeUseCases.Create;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.UeUseCases.Csv;
using System.Text;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UeController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/<UeController>
    [HttpGet]
    public async Task<ActionResult<List<UeDto>>> Get()
    {
        GetUeUseCase uc = new GetUeUseCase(repositoryFactory);
        List<Ue> ues = await uc.ExecuteAsync();
        return Ok(ues.Select(u => new UeDto().ToDto(u)));
    }

    // GET api/<UeController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UeDto>> Get(long id)
    {
        GetUeUseCase uc = new GetUeUseCase(repositoryFactory);
        try
        {
            Ue ue = await uc.ExecuteAsync(id);
            return Ok(new UeDto().ToDto(ue));
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST api/<UeController>
    [HttpPost]
    public async Task<ActionResult<UeDto>> PostAsync([FromBody] UeDto dto)
    {
        CreateUeUseCase uc = new CreateUeUseCase(repositoryFactory);
        Ue ue = dto.ToEntity();
        Ue created = await uc.ExecuteAsync(ue);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new UeDto().ToDto(created));
    }

    // PUT api/<UeController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(long id, [FromBody] UeDto dto)
    {
        UpdateUeUseCase uc = new UpdateUeUseCase(repositoryFactory);
        try
        {
            await uc.ExecuteAsync(id, dto.NumeroUe, dto.Intitule);
            return NoContent();
        }
        catch (UeNotFoundException)
        {
            return NotFound();
        }
    }

    // POST api/Ue/5/Parcours
    [HttpPost("{id}/Parcours")]
    public async Task<ActionResult<UeDto>> AddParcours(long id, [FromBody] List<long> parcoursIds)
    {
        AddParcoursToUeUseCase uc = new AddParcoursToUeUseCase(repositoryFactory);
        try
        {
            Ue ue = await uc.ExecuteAsync(id, parcoursIds.ToArray());
            return Ok(new UeDto().ToDto(ue));
        }
        catch (UeNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE api/<UeController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        DeleteUeUseCase uc = new DeleteUeUseCase(repositoryFactory);
        await uc.ExecuteAsync(id);
        return NoContent();
    }

    // GET: api/Ue/5/notes/csv
    [HttpGet("{id}/notes/csv")]
    public async Task<IActionResult> GetCsv(long id)
    {
        GenerateCsvNotesUseCase uc = new GenerateCsvNotesUseCase(repositoryFactory);
        try
        {
            byte[] csvData = await uc.ExecuteAsync(id);
            return File(csvData, "text/csv", $"notes_ue_{id}.csv");
        }
        catch (UeNotFoundException)
        {
            return NotFound();
        }
    }

    // POST: api/Ue/5/notes/csv
    [HttpPost("{id}/notes/csv")]
    public async Task<IActionResult> PostCsv(long id, IFormFile file)
    {
        try 
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using (var stream = file.OpenReadStream())
            {
                 SaisirNotesUseCase uc = new SaisirNotesUseCase(repositoryFactory);
                 await uc.ExecuteAsync(id, stream);
            }
            return NoContent();
        }
        catch (UeNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
