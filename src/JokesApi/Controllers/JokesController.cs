using System.Collections.Generic;
using System.Threading.Tasks;

using JokesApi.Services;

using JokesApiContracts.Domain.Model;

using Microsoft.AspNetCore.Mvc;

namespace JokesApi.Controllers
{
    [Route("api/jokes")]
    public class JokesController : Controller
    {
        private readonly IJokesService service;

        public JokesController(
            IJokesService service)
        {
            this.service = service;
        }

        [HttpGet("languages")]
        public async Task<IEnumerable<JokesLanguageModel>> GetLanguagesAsync()
        {
            return await this.service.GetLanguagesAsync(this.HttpContext.RequestAborted);
        }

        [HttpGet("{language}/{category}")]
        public async Task<IEnumerable<JokeModel>> GetJokesAsync(
            string language,
            string category)
        {
            return await this.service.GetJokesAsync(
                language,
                category,
                this.HttpContext.RequestAborted);
        }

        [HttpPost("import")]
        public async Task ImportJokesAsync(
            [FromBody] IEnumerable<JokeImportModel> importJokes)
        {
            await this.service.ImportJokesAsync(importJokes, this.HttpContext.RequestAborted);
        }
    }
}