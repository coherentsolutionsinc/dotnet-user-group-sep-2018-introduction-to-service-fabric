using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.ServiceFabric.Services.Runtime;

namespace JokesApi
{
    public static class Program
    {
        public static void Main(
            string[] args)
        {
                try
                {
                    ServiceRuntime.RegisterServiceAsync(
                            "JokesApiServiceType",
                            context => new JokesApiService(context))
                       .GetAwaiter()
                       .GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(
                        Process.GetCurrentProcess().Id,
                        typeof(JokesApiService).Name);

                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                    throw;
                }
        }
    }
}