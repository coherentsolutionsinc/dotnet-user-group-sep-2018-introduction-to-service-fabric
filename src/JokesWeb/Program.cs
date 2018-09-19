using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.ServiceFabric.Services.Runtime;

namespace JokesWeb
{
    public class Program
    {
        public static void Main(
            string[] args)
        {
            try
            {
                ServiceRuntime.RegisterServiceAsync(
                        "JokesWebServiceType",
                        context => new JokesWebService(context))
                   .GetAwaiter()
                   .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(
                    Process.GetCurrentProcess().Id,
                    typeof(JokesWebService).Name);

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