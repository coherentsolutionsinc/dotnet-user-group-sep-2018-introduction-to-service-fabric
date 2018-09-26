using System.Collections.Generic;
using System.Fabric;
using System.IO;

using JokesWeb.Clients;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace JokesWeb
{
    /// <summary>
    ///     The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class JokesWebService : StatelessService
    {
        public JokesWebService(
            StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        ///     Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(
                    serviceContext =>
                        new KestrelCommunicationListener(
                            serviceContext,
                            "ServiceEndpoint", // Name of endpoint (must match one defined in ServiceManifest.xml)
                            (
                                url,
                                listener) =>
                            {
                                return new WebHostBuilder()
                                   .UseKestrel()
                                   .ConfigureServices(
                                        services =>
                                        {
                                            services.AddSingleton(serviceContext);
                                            services.AddSingleton<IJokesApiClient, FabricJokesApiClient>();
                                            services.AddSingleton<IJokesStatsClient, FabricJokesStatsClient>();
                                        })
                                   .UseContentRoot(Directory.GetCurrentDirectory())
                                   .UseStartup<Startup>()
                                   .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                   .UseUrls(url)
                                   .Build();
                            }),
                    "ServiceEndpoint")
            };
        }
    }
}