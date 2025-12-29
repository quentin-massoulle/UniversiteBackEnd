using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;

namespace UniversiteRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController(IRepositoryFactory repositoryFactory)  : ControllerBase
    {
        // GET: api/<EtudiantController>
        [HttpGet]
        public async Task Get()
        {

        }

        
        // GET api/<EtudiantApi>/5
        [HttpGet("{id}")]
        public async Task Get(long id)
        {
            

        }

        // Crée un nouvel étudiant sans parcours
        // POST api/<EtudiantApi>
        [HttpPost]
        public async Task PostAsync([FromBody] Etudiant etudiant)
        {
            
        }

        // PUT api/<EtudiantController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EtudiantController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            
        }
    }
}
