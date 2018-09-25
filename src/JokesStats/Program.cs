using System;
using System.Threading;

using Microsoft.ServiceFabric.Actors.Runtime;

namespace JokesStats
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                ActorRuntime.RegisterActorAsync<JokesStats>(
                        (context, actorType) => new ActorService(context, actorType))
                   .GetAwaiter()
                   .GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}