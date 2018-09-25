using System.Threading;
using System.Threading.Tasks;

using JokesStats.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace JokesStats
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class JokesStats : Actor, IJokesStats
    {
        /// <summary>
        ///     Initializes a new instance of JokesStats
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public JokesStats(
            ActorService actorService,
            ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return this.StateManager.TryAddStateAsync("viewed", 0);
        }

        public Task IncrementViewed(
            CancellationToken cancellationToken)
        {
            return this.StateManager.AddOrUpdateStateAsync(
                "viewed",
                0, (key, value) => ++value,
                cancellationToken);
        }

        public async Task<JokesStatistics> GetAsync(
            CancellationToken cancellationToken)
        {
            return new JokesStatistics
            {
                ViewedCount = await this.StateManager.GetStateAsync<int>("viewed", cancellationToken)
            };
        }
    }
}