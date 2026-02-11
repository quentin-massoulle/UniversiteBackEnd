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
using UniversiteDomain.Exceptions.NoteExeptions;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.Exceptions.ParcoursExeptions;
using UniversiteDomain.UseCases.SecurityUseCases.Create;
using UniversiteDomain.UseCases.SecurityUseCases.Get;
using UniversiteEFDataProvider.Entities;


namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        // GET: api/<EtudiantController>
        [HttpGet]
        public async Task<ActionResult<List<EtudiantDto>>> GetLesEtudiant()
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            List<Etudiant> etudiants = await uc.ExecuteAsync();

            return Ok(etudiants.Select(e => new EtudiantDto().ToDto(e)));
        }

        
        // GET api/<EtudiantApi>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EtudiantDto>> GetUnEtudiant(long id)
        {
            GetEtudiantUseCase uc = new GetEtudiantUseCase(repositoryFactory);
            
            // On récupère l'étudiant via le UseCase
            try 
            {
                Etudiant etudiant = await uc.ExecuteAsync(id);
                return Ok(new EtudiantDto().ToDto(etudiant));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<EtudiantDto>> PostAsync([FromBody] EtudiantDto etudiantDto)
        {
            CreateEtudiantUseCase createEtudiantUc = new CreateEtudiantUseCase(repositoryFactory);
            CreateUniversiteUserUseCase createUserUc = new CreateUniversiteUserUseCase(repositoryFactory);

            string role="";
            string email="";
            IUniversiteUser user = null;
            CheckSecu(out role, out email, out user);
            if (!createUserUc.IsAuthorized(role) || !createUserUc.IsAuthorized(role)) return Unauthorized();
    
            Etudiant etud = etudiantDto.ToEntity();
    
            try
            {
                etud = await createEtudiantUc.ExecuteAsync(etud);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }

            try
            {
                // Création du user associé
                user = new UniversiteUser { UserName = etudiantDto.Email, Email = etudiantDto.Email, Etudiant = etud };
                // Un créé l'utilisateur avec un mot de passe par défaut et un rôle étudiant
                await createUserUc.ExecuteAsync(etud.Email, etud.Email, "Miage2025#", Roles.Etudiant, etud); 
            }
            catch (Exception e)
            {
                // On supprime l'étudiant que l'on vient de créer. Sinon on a un étudiant mais pas de user associé
                await new DeleteEtudiantUseCase(repositoryFactory).ExecuteAsync(etud.Id);
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }

            EtudiantDto dto = new EtudiantDto().ToDto(etud);
            return CreatedAtAction(nameof(GetUnEtudiant), new { id = dto.Id }, dto);
        }

        // PUT api/<EtudiantController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] EtudiantDto dto)
        {
            UpdateEtudiantUseCase uc = new UpdateEtudiantUseCase(repositoryFactory);
            try
            {
                await uc.ExecuteAsync(id, dto.NumEtud, dto.Nom, dto.Prenom, dto.Email);
                return NoContent();
            }
            catch (EtudiantNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e) when (e is DuplicateNumEtudException || e is InvalidEmailException || e is DuplicateEmailException || e is InvalidNomEtudiantException)
            {
                return BadRequest(e.Message);
            }
        }

        // POST api/<EtudiantController>/5/Note
        [HttpPost("{id}/Note")]
        public async Task<ActionResult<NoteDto>> AjouterNoteEtudiant(long id, [FromBody] NoteDto dto)
        {
            CreateNoteUseCase uc = new CreateNoteUseCase(repositoryFactory);
            Note note = dto.ToEntity();
            note.EtudiantId = id;
            
            try
            {
                Note created = await uc.ExecuteAsync(note);
                return CreatedAtAction("Get", "Note", new { etudiantId = created.EtudiantId, ueId = created.UeId }, new NoteDto().ToDto(created));
            }
            catch (Exception e) when (e is InvalidValeurNoteException || e is InvalidIdUe || e is EtudiantNotFoundException || e is DuplicateNoteUeException)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE api/<EtudiantController>/5
        [HttpDelete("{id}")]
        public async Task DeleteUnEtudiant(long id)
        {
            DeleteEtudiantUseCase uc=new DeleteEtudiantUseCase(repositoryFactory);
            await uc.ExecuteAsync(id);
        }

        // POST api/Etudiant/5/Parcours
        [HttpPost("{id}/Parcours")]
        public async Task<ActionResult<EtudiantDto>> AddParcoursEtudiant(long id, [FromBody] long parcoursId)
        {
            AddEtudiantDansParcoursUseCase uc = new AddEtudiantDansParcoursUseCase(repositoryFactory);
            try
            {
                // Note: The use case returns the Parcours, but we want to return the updated Etudiant.
                // We need to fetch the student again to get the updated state with the Parcours.
                await uc.ExecuteAsync(parcoursId, id);
                
                GetEtudiantUseCase getUc = new GetEtudiantUseCase(repositoryFactory);
                Etudiant etudiant = await getUc.ExecuteAsync(id);
                
                return Ok(new EtudiantDto().ToDto(etudiant));
            }
            catch (Exception e) when (e is EtudiantNotFoundException || e is ParcoursNotFoundException)
            {
                return NotFound(e.Message);
            }
            catch (DuplicateInscriptionException e)
            {
                return BadRequest(e.Message);
            }
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
