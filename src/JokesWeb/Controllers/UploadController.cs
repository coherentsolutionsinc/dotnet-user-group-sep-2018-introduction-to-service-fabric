using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using JokesApiContracts.Domain.Model;

using JokesWeb.Clients;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace JokesWeb.Controllers
{
    public class UploadController : Controller
    {
        private readonly IJokesApiClient jokesWebServiceClient;

        public UploadController(
            IJokesApiClient jokesWebServiceClient)
        {
            this.jokesWebServiceClient = jokesWebServiceClient;
        }

        public async Task<IActionResult> Upload(
            List<IFormFile> files,
            [FromQuery] string language,
            [FromQuery] string category)
        {
            var serializer = new JsonSerializer();

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    continue;
                }

                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    using (var jsonReader = new JsonTextReader(stream))
                    {
                        var importJokes = serializer.Deserialize<JokeImportModel[]>(jsonReader);

                        await this.jokesWebServiceClient.ImportJokesAsync(importJokes, this.HttpContext.RequestAborted);
                    }
                }
            }

            return this.RedirectToAction("Index", "Home", new { language, category });
        }
    }
}