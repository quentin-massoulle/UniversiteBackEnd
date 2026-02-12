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
public class CsvController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/Csv/5
    [HttpGet("{idUe}")]
    public async Task<IActionResult> GetCsv(long idUe)
    {
        try
        {
            CheckSecu(out string role, out _, out _);
            if (role != Roles.Scolarite) return Forbid();

            GenerateCsvNotesUseCase uc = new GenerateCsvNotesUseCase(repositoryFactory);
            byte[] csvData = await uc.ExecuteAsync(idUe);
            return File(csvData, "text/csv", $"notes_ue_{idUe}.csv"); 
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (UeNotFoundException) { return NotFound(); }
    }

    // GET: api/Csv/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCsv()
    {
        try
        {
            CheckSecu(out string role, out _, out _);
            if (role != Roles.Scolarite) return Forbid();

            GenerateCsvGlobalUseCase uc = new GenerateCsvGlobalUseCase(repositoryFactory);
            byte[] csvData = await uc.ExecuteAsync();
            return File(csvData, "text/csv", "notes_global.csv"); 
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    // POST: api/Csv
    [HttpPost]
    public async Task<IActionResult> PostCsv(IFormFile file)
    {
        try 
        {
            CheckSecu(out string role, out _, out _);
            if (role != Roles.Scolarite) return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using (var stream = file.OpenReadStream())
            {
                 SaisirNotesUseCase uc = new SaisirNotesUseCase(repositoryFactory);
                 await uc.ExecuteAsync(stream);
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (UeNotFoundException) { return NotFound(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    private void CheckSecu(out string role, out string email, out IUniversiteUser user)
    {
        role = "";
        ClaimsPrincipal claims = HttpContext.User;
        if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
        if (claims.FindFirst(ClaimTypes.Email) == null) throw new UnauthorizedAccessException();
        email = claims.FindFirst(ClaimTypes.Email).Value;
        if (email == null) throw new UnauthorizedAccessException();
        
        user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
        if (user == null) throw new UnauthorizedAccessException();
        
        if (claims.FindFirst(ClaimTypes.Role) == null) throw new UnauthorizedAccessException();
        var ident = claims.Identities.FirstOrDefault();
        if (ident == null) throw new UnauthorizedAccessException();
        role = ident.FindFirst(ClaimTypes.Role).Value;
        if (role == null) throw new UnauthorizedAccessException();
        
        bool isInRole = new IsInRoleUseCase(repositoryFactory).ExecuteAsync(email, role).Result; 
        if (!isInRole) throw new UnauthorizedAccessException();
    }

}
