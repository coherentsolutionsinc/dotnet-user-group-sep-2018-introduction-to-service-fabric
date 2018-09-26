using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using JokesApiContracts.Domain.Model;

using Microsoft.ServiceFabric.Services.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JokesWeb.Clients
{
    public class FabricJokesApiClient : IJokesApiClient
    {
        public async Task<IEnumerable<JokesLanguageModel>> GetLanguagesAsync(
            CancellationToken cancellationToken)
        {
            var tuples = await this.ResolveServicesAsync();

            var languages = new List<JokesLanguageModel>();

            foreach (var tuple in tuples)
            {
                languages.AddRange(
                    await this.GetResponseAsync<JokesLanguageModel[]>(tuple.serviceName, tuple.partitionName, "api/jokes/languages", cancellationToken));
            }

            return languages
               .GroupBy(language => language.Name)
               .Select(
                    group => new JokesLanguageModel
                    {
                        Name = group.Key,
                        Categories = group.SelectMany(language => language.Categories).ToArray()
                    })
               .ToArray();
        }

        public async Task<IEnumerable<JokeModel>> GetJokesAsync(
            string language,
            string category,
            CancellationToken cancellationToken)
        {
            var tuple = await this.ResolveServiceAsync(language, category);

            return await this.GetResponseAsync<JokeModel[]>(tuple.serviceName, tuple.partitionName, $"api/jokes/{language}/{category}", cancellationToken);
        }

        public async Task ImportJokesAsync(
            IEnumerable<JokeImportModel> importJokes,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient();

            var serializer = JsonSerializer.Create();

            foreach (var byLanguage in importJokes.GroupBy(importJoke => importJoke.Language))
            {
                foreach (var byCategory in byLanguage.GroupBy(importJoke => importJoke.Category))
                {
                    var tuple = await this.ResolveServiceAsync(byLanguage.Key, byCategory.Key);
                    var partition = await ServicePartitionResolver.GetDefault().ResolveAsync(
                        tuple.serviceName,
                        new ServicePartitionKey(tuple.partitionName),
                        cancellationToken);

                    // Find endpoint address information of primary replica
                    var address = partition.Endpoints.Single(e => e.Role == ServiceEndpointRole.StatefulPrimary)?.Address;

                    var value = JObject.Parse(address)["Endpoints"]?["ServiceEndpoint"]?.Value<string>();

                    var builder = new StringBuilder();

                    using (var writer = new StringWriter(builder))
                    {
                        serializer.Serialize(writer, byCategory);
                    }

                    var response = await client.PostAsync(
                        $"{value}/api/jokes/import",
                        new StringContent(builder.ToString(), Encoding.UTF8, "application/json"),
                        cancellationToken);

                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task<(Uri serviceName, string partitionName)> ResolveServiceAsync(
            string language,
            string category)
        {
            var tuples = await this.ResolveServicesAsync();

            foreach (var tuple in tuples)
            {
                var serviceName = WebUtility.UrlDecode(tuple.serviceName.AbsoluteUri);
                if ((serviceName.Contains($"/{language}/") || serviceName.EndsWith($"/{language}")) && tuple.partitionName.Equals(category))
                {
                    return tuple;
                }
            }

            return default((Uri serviceName, string partitionName));
        }

        private async Task<IEnumerable<(Uri serviceName, string partitionName)>> ResolveServicesAsync()
        {
            var client = new FabricClient();

            // Get a list of all service objects of out application object
            var services = await client.QueryManager.GetServiceListAsync(new Uri($"fabric:/JokesApp"));

            var result = new List<(Uri serviceName, string partitionName)>();

            // Get only service objects projected from 'JokesApiServiceType'
            foreach (var service in services.Where(service => service.ServiceTypeName == "JokesApiServiceType"))
            {
                // Get a list of all partitions of service object
                var partitionInfos = await client.QueryManager.GetPartitionListAsync(service.ServiceName);

                foreach (var partitionInfo in partitionInfos)
                {
                    var serviceName = service.ServiceName;
                    var partitionName = ((NamedPartitionInformation) partitionInfo.PartitionInformation).Name;

                    result.Add((serviceName, partitionName));
                }
            }

            return result;
        }

        private async Task<T> GetResponseAsync<T>(
            Uri serviceName,
            string partitionName,
            string resource,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient();

            // Resolve information about 'Partition'
            var partition = await ServicePartitionResolver.GetDefault().ResolveAsync(serviceName, new ServicePartitionKey(partitionName), cancellationToken);

            // Get endpoint address information and retrieve ServiceEndpoint's address in particular.
            var address = partition.GetEndpoint().Address;
            var value = JObject.Parse(address)["Endpoints"]?["ServiceEndpoint"]?.Value<string>();

            var response = await client.GetAsync($"{value}/{resource}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var serializer = JsonSerializer.Create();
            using (var reader = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}