using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.ParcoursUseCase;
using UniversiteDomain.UseCases.ParcoursUseCase.Delete;
using UniversiteDomain.UseCases.ParcoursUseCase.Get;
using UniversiteDomain.UseCases.ParcoursUseCase.Update;
using UniversiteDomain.Exceptions.ParcoursExeptions;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ParcoursController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/<ParcoursController>
    [HttpGet]
    public async Task<ActionResult<List<Parcours>>> Get()
    {
        GetParcoursUseCase uc = new GetParcoursUseCase(repositoryFactory);
        List<Parcours> parcours = await uc.ExecuteAsync();
        return Ok(parcours);
    }

    // GET api/<ParcoursController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Parcours>> Get(long id)
    {
        GetParcoursUseCase uc = new GetParcoursUseCase(repositoryFactory);
        Parcours? parcours = await uc.ExecuteAsync(id);
        if (parcours == null) return NotFound();
        return Ok(parcours);
    }

    // POST api/<ParcoursController>
    [HttpPost]
    public async Task<ActionResult<Parcours>> PostAsync([FromBody] Parcours parcours)
    {
        CreateParcoursUseCase uc = new CreateParcoursUseCase(repositoryFactory);
        Parcours created = await uc.ExecuteAsync(parcours);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT api/<ParcoursController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult<Parcours>> Put(long id, [FromBody] Parcours parcours)
    {
        UpdateParcoursUseCase uc = new UpdateParcoursUseCase(repositoryFactory);
        try
        {
            await uc.ExecuteAsync(id, parcours.NomParcours, parcours.AnneeFormation);
            return NoContent();
        }
        catch (ParcoursNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE api/<ParcoursController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        DeleteParcoursUseCase uc = new DeleteParcoursUseCase(repositoryFactory);
        await uc.ExecuteAsync(id);
        return NoContent();
    }
}
