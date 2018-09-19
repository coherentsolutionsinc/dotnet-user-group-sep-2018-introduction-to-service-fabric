using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JokesApiContracts.Domain.Model;

namespace JokesApi.Services
{
    public interface IJokesService
    {
        Task<IEnumerable<JokesLanguageModel>> GetLanguagesAsync(
            CancellationToken cancellationToken);

        Task<IEnumerable<JokeModel>> GetJokesAsync(
            string language,
            string category,
            CancellationToken cancellationToken);

        Task ImportJokesAsync(
            IEnumerable<JokeImportModel> importJokes,
            CancellationToken cancellationToken);
    }
}