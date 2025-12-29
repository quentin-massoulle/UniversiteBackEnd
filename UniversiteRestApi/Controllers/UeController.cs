using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCase.Delete;
using UniversiteDomain.UseCases.UeUseCase.Get;
using UniversiteDomain.UseCases.UeUseCase.Update;
using UniversiteDomain.UseCases.UeUseCases.Create;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.Dtos;

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
    public async Task<ActionResult<UeDto>> AddParcours(long id, [FromBody] long parcoursId)
    {
        AddParcoursToUeUseCase uc = new AddParcoursToUeUseCase(repositoryFactory);
        try
        {
            Ue ue = await uc.ExecuteAsync(id, parcoursId);
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
}
