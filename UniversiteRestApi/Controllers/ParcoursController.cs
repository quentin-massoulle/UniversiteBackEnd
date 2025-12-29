using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.ParcoursUseCase;
using UniversiteDomain.UseCases.ParcoursUseCase.Delete;
using UniversiteDomain.UseCases.ParcoursUseCase.Get;
using UniversiteDomain.UseCases.ParcoursUseCase.Update;
using UniversiteDomain.Exceptions.ParcoursExeptions;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ParcoursController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/<ParcoursController>
    [HttpGet]
    public async Task<ActionResult<List<ParcoursDto>>> Get()
    {
        GetParcoursUseCase uc = new GetParcoursUseCase(repositoryFactory);
        List<Parcours> parcours = await uc.ExecuteAsync();
        return Ok(parcours.Select(p => new ParcoursDto().ToDto(p)));
    }

    // GET api/<ParcoursController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ParcoursDto>> Get(long id)
    {
        GetParcoursUseCase uc = new GetParcoursUseCase(repositoryFactory);
        Parcours? parcours = await uc.ExecuteAsync(id);
        if (parcours == null) return NotFound();
        return Ok(new ParcoursDto().ToDto(parcours));
    }

    // POST api/<ParcoursController>
    [HttpPost]
    public async Task<ActionResult<ParcoursDto>> PostAsync([FromBody] ParcoursDto dto)
    {
        CreateParcoursUseCase uc = new CreateParcoursUseCase(repositoryFactory);
        Parcours parcours = dto.ToEntity();
        Parcours created = await uc.ExecuteAsync(parcours);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new ParcoursDto().ToDto(created));
    }

    // PUT api/<ParcoursController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(long id, [FromBody] ParcoursDto dto)
    {
        UpdateParcoursUseCase uc = new UpdateParcoursUseCase(repositoryFactory);
        try
        {
            await uc.ExecuteAsync(id, dto.NomParcours, dto.AnneeFormation);
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

    // POST api/Parcours/5/Etudiants
    [HttpPost("{id}/Etudiants")]
    public async Task<ActionResult<ParcoursDto>> AddEtudiants(long id, [FromBody] List<long> etudiantIds)
    {
        AddEtudiantDansParcoursUseCase uc = new AddEtudiantDansParcoursUseCase(repositoryFactory);
        try
        {
            Parcours parcours = await uc.ExecuteAsync(id, etudiantIds.ToArray());
            return Ok(new ParcoursDto().ToDto(parcours));
        }
        catch (ParcoursNotFoundException)
        {
            return NotFound();
        }
        catch (EtudiantNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (DuplicateInscriptionException e)
        {
            return BadRequest(e.Message);
        }
    }

    // POST api/Parcours/5/Ues
    [HttpPost("{id}/Ues")]
    public async Task<ActionResult<ParcoursDto>> AddUes(long id, [FromBody] List<long> ueIds)
    {
        AddUeDansParcoursUseCase uc = new AddUeDansParcoursUseCase(repositoryFactory);
        try
        {
            Parcours parcours = await uc.ExecuteAsync(id, ueIds.ToArray());
            return Ok(new ParcoursDto().ToDto(parcours));
        }
        catch (ParcoursNotFoundException)
        {
            return NotFound();
        }
        catch (UeNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (DuplicateUeDansParcoursException e)
        {
            return BadRequest(e.Message);
        }
    }
}
