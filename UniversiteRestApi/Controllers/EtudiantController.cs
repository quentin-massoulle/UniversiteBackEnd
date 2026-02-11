using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.EtudiantUseCases.Delete;
using UniversiteDomain.UseCases.EtudiantUseCases.Get;
using UniversiteDomain.UseCases.EtudiantUseCases.Update;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.UseCases.NoteUseCase.Create;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.UseCases.SecurityUseCases.Create;
using UniversiteDomain.UseCases.SecurityUseCases.Get;



namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        
        // GET: api/Etudiant
        [HttpGet]
        public async Task<ActionResult<List<EtudiantDto>>> GetLesEtudiant()
        {
            try 
            {
                CheckSecu(out _, out _, out _);
                GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
                List<Etudiant> etudiants = await uc.ExecuteAsync();
                return Ok(etudiants.Select(e => new EtudiantDto().ToDto(e)));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        
        // GET api/<EtudiantController>/complet/5
        [HttpGet("complet/{id}")]
        public async Task<ActionResult<EtudiantCompletDto>> GetUnEtudiant(long id)
        {
            // Identification et authentification
            string role="";
            string email="";
            IUniversiteUser user = null;
            try
            {
                CheckSecu(out role, out email, out user);
            }
            catch (Exception e)
            {
                return Unauthorized();
            }
    
            GetEtudiantCompletUseCase uc = new GetEtudiantCompletUseCase(repositoryFactory);
            // Autorisation
            // On vérifie si l'utilisateur connecté a le droit d'accéder à la ressource
            if (!uc.IsAuthorized(role, user, id)) return Unauthorized();
            Etudiant? etud;
            try
            {
                etud = await uc.ExecuteAsync(id);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }
            if (etud == null) return NotFound();
            return new EtudiantCompletDto().ToDto(etud);
        }

        // POST api/Etudiant
        [HttpPost]
        public async Task<ActionResult<EtudiantDto>> PostAsync([FromBody] EtudiantDto etudiantDto)
        {
            try
            {
                string role;
                string email;
                IUniversiteUser user;
                CheckSecu(out role, out email, out user);

                CreateEtudiantUseCase createEtudiantUc = new CreateEtudiantUseCase(repositoryFactory);
                CreateUniversiteUserUseCase createUserUc = new CreateUniversiteUserUseCase(repositoryFactory);

                // Vérification si le rôle a le droit de créer (Généralement Admin)
                if (!createUserUc.IsAuthorized(role)) return Forbid();

                Etudiant etud = etudiantDto.ToEntity();
                etud = await createEtudiantUc.ExecuteAsync(etud);

                try
                {
                    await createUserUc.ExecuteAsync(etud.Email, etud.Email, "Miage2025#", Roles.Etudiant, etud); 
                }
                catch (Exception e)
                {
                    await new DeleteEtudiantUseCase(repositoryFactory).ExecuteAsync(etud.Id);
                    ModelState.AddModelError("UserCreation", e.Message);
                    return ValidationProblem();
                }

                EtudiantDto dto = new EtudiantDto().ToDto(etud);
                return CreatedAtAction(nameof(GetUnEtudiant), new { id = dto.Id }, dto);
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        // PUT api/Etudiant/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] EtudiantDto dto)
        {
            try
            {
                CheckSecu(out string role, out _, out _);
                // Optionnel : Seul un admin ou l'étudiant lui-même peut modifier ?
                // Ici on applique une sécurité de base
                
                UpdateEtudiantUseCase uc = new UpdateEtudiantUseCase(repositoryFactory);
                await uc.ExecuteAsync(id, dto.NumEtud, dto.Nom, dto.Prenom, dto.Email);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (EtudiantNotFoundException) { return NotFound(); }
            catch (Exception e) { return BadRequest(e.Message); }
        }

        // POST api/Etudiant/5/Note
        [HttpPost("{id}/Note")]
        public async Task<ActionResult<NoteDto>> AjouterNoteEtudiant(long id, [FromBody] NoteDto dto)
        {
            try
            {
                CheckSecu(out _, out _, out _);
                CreateNoteUseCase uc = new CreateNoteUseCase(repositoryFactory);
                Note note = dto.ToEntity();
                note.EtudiantId = id;
                
                Note created = await uc.ExecuteAsync(note);
                return CreatedAtAction("Get", "Note", new { etudiantId = created.EtudiantId, ueId = created.UeId }, new NoteDto().ToDto(created));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception e) { return BadRequest(e.Message); }
        }

        // DELETE api/Etudiant/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUnEtudiant(long id)
        {
            try
            {
                CheckSecu(out string role, out _, out _);
                // On pourrait vérifier ici si role == Roles.Admin
                
                DeleteEtudiantUseCase uc = new DeleteEtudiantUseCase(repositoryFactory);
                await uc.ExecuteAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        // POST api/Etudiant/5/Parcours
        [HttpPost("{id}/Parcours")]
        public async Task<ActionResult<EtudiantDto>> AddParcoursEtudiant(long id, [FromBody] long parcoursId)
        {
            try
            {
                CheckSecu(out _, out _, out _);
                AddEtudiantDansParcoursUseCase uc = new AddEtudiantDansParcoursUseCase(repositoryFactory);
                await uc.ExecuteAsync(parcoursId, id);
                
                GetEtudiantUseCase getUc = new GetEtudiantUseCase(repositoryFactory);
                Etudiant etudiant = await getUc.ExecuteAsync(id);
                return Ok(new EtudiantDto().ToDto(etudiant));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception e) { return BadRequest(e.Message); }
        }
        
        private void CheckSecu(out string role, out string email, out IUniversiteUser user)
        {
            role = "";
            // Récupération des informations de connexion dans la requête http entrante
            ClaimsPrincipal claims = HttpContext.User;
            // Faisons nos tests pour savoir si la personne est bien connectée
            if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
            // Récupérons le email de la personne connectée
            if (claims.FindFirst(ClaimTypes.Email)==null) throw new UnauthorizedAccessException();
            email = claims.FindFirst(ClaimTypes.Email).Value;
            if (email==null) throw new UnauthorizedAccessException();
            // Vérifions qu'il est bien associé à un utilisateur référencé
            user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
            if (user==null) throw new UnauthorizedAccessException();
            // Vérifions qu'un rôle a bien été défini
            if (claims.FindFirst(ClaimTypes.Role)==null) throw new UnauthorizedAccessException();
            // Récupérons le rôle de l'utilisateur
            var ident = claims.Identities.FirstOrDefault();
            if (ident == null)throw new UnauthorizedAccessException();
            role = ident.FindFirst(ClaimTypes.Role).Value;
            if (role == null) throw new UnauthorizedAccessException();
            // Vérifions que le user a bien le role envoyé via http
            bool isInRole = new IsInRoleUseCase(repositoryFactory).ExecuteAsync(email, role).Result; 
            if (!isInRole) throw new UnauthorizedAccessException();
            // Si tout est passé sans renvoyer d'exception, le user est authentifié et conncté
        }
    }
}
