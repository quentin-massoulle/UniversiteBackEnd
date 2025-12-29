using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCase.Delete;
using UniversiteDomain.UseCases.UeUseCase.Get;
using UniversiteDomain.UseCases.UeUseCase.Update;
using UniversiteDomain.UseCases.UeUseCases.Create;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UeController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/<UeController>
    [HttpGet]
    public async Task<ActionResult<List<Ue>>> Get()
    {
        GetUeUseCase uc = new GetUeUseCase(repositoryFactory);
        List<Ue> ues = await uc.ExecuteAsync();
        return Ok(ues);
    }

    // GET api/<UeController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Ue>> Get(long id)
    {
        GetUeUseCase uc = new GetUeUseCase(repositoryFactory);
        try
        {
            Ue ue = await uc.ExecuteAsync(id);
            return Ok(ue);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST api/<UeController>
    [HttpPost]
    public async Task<ActionResult<Ue>> PostAsync([FromBody] Ue ue)
    {
        CreateUeUseCase uc = new CreateUeUseCase(repositoryFactory);
        Ue created = await uc.ExecuteAsync(ue);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT api/<UeController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult<Ue>> Put(long id, [FromBody] Ue ue)
    {
        UpdateUeUseCase uc = new UpdateUeUseCase(repositoryFactory);
        try
        {
            await uc.ExecuteAsync(id, ue.NumeroUe, ue.Intitule);
            return NoContent();
        }
        catch (UeNotFoundException)
        {
            return NotFound();
        }
    // POST api/Ue/5/Parcours
    [HttpPost("{id}/Parcours")]
    public async Task<ActionResult<Ue>> AddParcours(long id, [FromBody] long parcoursId)
    {
        AddParcoursToUeUseCase uc = new AddParcoursToUeUseCase(repositoryFactory);
        try
        {
            Ue ue = await uc.ExecuteAsync(id, parcoursId);
            return Ok(ue);
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
