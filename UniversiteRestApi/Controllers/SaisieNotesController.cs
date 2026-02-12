using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.UseCases.UeUseCases.Csv;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.SecurityUseCases.Get;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SaisieNotesController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/SaisieNotes/ue/5/csv
    [HttpGet("ue/{idUe}/csv")]
    public async Task<IActionResult> GetCsv(long idUe)
    {
        GenerateCsvNotesUseCase uc = new GenerateCsvNotesUseCase(repositoryFactory);
        byte[] csvData = await uc.ExecuteAsync(idUe);
        return File(csvData, "text/csv", $"notes_ue_{idUe}.csv"); 
    }

    // POST: api/SaisieNotes/ue/5/csv
    [HttpPost("ue/{idUe}/csv")]
    public async Task<IActionResult> PostCsv(long idUe, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        using (var stream = file.OpenReadStream())
        {
            SaisirNotesUseCase uc = new SaisirNotesUseCase(repositoryFactory);
            await uc.ExecuteAsync(idUe, stream);
        }
        return NoContent();
    }

}
