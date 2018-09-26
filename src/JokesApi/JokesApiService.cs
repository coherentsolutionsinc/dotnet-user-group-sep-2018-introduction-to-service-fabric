using System.Collections.Generic;
using System.Fabric;

using JokesApi.Services;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace JokesApi
{
    /// <summary>
    ///     The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class JokesApiService : StatefulService
    {
        public JokesApiService(
            StatefulServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        ///     Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(
                    serviceContext =>
                        new KestrelCommunicationListener(
                            serviceContext,
                            "ServiceEndpoint",
                            (
                                url,
                                listener) =>
                            {
                                return WebHost.CreateDefaultBuilder()
                                   .ConfigureServices(
                                        services =>
                                        {
                                            services.AddSingleton(serviceContext);
                                            services.AddSingleton(this.Partition);
                                            services.AddSingleton(this.StateManager);

                                            services.AddSingleton<IJokesService, FabricJokesService>();
                                        })
                                   .UseStartup<Startup>()
                                   .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                   .UseUrls(url)
                                   .Build();
                            }),
                    "ServiceEndpoint",
                    true)
            };
        }
    }
}