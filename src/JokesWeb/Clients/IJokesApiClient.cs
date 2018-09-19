using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JokesApiContracts.Domain.Model;

namespace JokesWeb.Clients
{
    public interface IJokesApiClient
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