using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]

namespace JokesStats.Interfaces
{
    public interface IJokesStats : IActor
    {
        Task IncrementViewed(
            CancellationToken cancellationToken);

        Task<JokesStatistics> GetAsync(
            CancellationToken cancellationToken);
    }
}