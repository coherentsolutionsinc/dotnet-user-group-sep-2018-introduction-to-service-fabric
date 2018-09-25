using System.Threading;
using System.Threading.Tasks;

using JokesStats.Interfaces;

namespace JokesWeb.Clients
{
    public interface IJokesStatsClient
    {
        Task<JokesStatistics> IncrementViewedAndGetStatisticsAsync(
            string user,
            string language,
            string category,
            CancellationToken cancellationToken);
    }
}