using System;
using System.Threading;
using System.Threading.Tasks;

using JokesStats.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace JokesWeb.Clients
{
    public class FabricJokesStatsClient : IJokesStatsClient
    {
        public async Task<JokesStatistics> IncrementViewedAndGetStatisticsAsync(
            string user,
            string language,
            string category,
            CancellationToken cancellationToken)
        {
            var actor = ActorProxy.Create<IJokesStats>(
                new ActorId($"{user}/{language}/{category}"),
                new Uri("fabric:/JokesApp/JokesStatsActorService"));

            await actor.IncrementViewed(cancellationToken);

            return await actor.GetAsync(cancellationToken);
        }
    }
}