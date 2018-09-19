using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using JokesApiContracts.Domain.Model;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace JokesApi.Services
{
    public class FabricJokesService : IJokesService
    {
        private const string COLLECTION_KEY = "jokes-collection";

        private readonly MapperConfiguration mapperConfiguration;

        private readonly StatefulServiceContext context;

        private readonly IStatefulServicePartition partition;

        private readonly IReliableStateManager manager;

        public FabricJokesService(
            StatefulServiceContext context,
            IStatefulServicePartition partition,
            IReliableStateManager manager)
        {
            this.mapperConfiguration = new MapperConfiguration(config => config.CreateMap<JokeImportModel, JokeModel>());

            this.context = context;
            this.partition = partition;
            this.manager = manager;
        }

        public async Task<IEnumerable<JokesLanguageModel>> GetLanguagesAsync(
            CancellationToken cancellationToken)
        {
            var jokes = await this.GetCollectionAsync();
            if (jokes.Count == 0)
            {
                return Array.Empty<JokesLanguageModel>();
            }

            var id = WebUtility.UrlDecode(this.context.ServiceName.AbsoluteUri)
               .Substring("fabric:/JokesApp/JokesApiService/".Length);

            var delimiter = id.IndexOf('/');

            var serviceName = delimiter > 0
                ? id.Substring(0, delimiter)
                : id;

            return new[]
            {
                new JokesLanguageModel
                {
                    Name = serviceName,
                    Categories = new[]
                    {
                        new JokesLanguageCategoryModel
                        {
                            Name = ((NamedPartitionInformation) this.partition.PartitionInfo).Name,
                            Count = jokes.Count
                        }
                    }
                }
            };
        }

        public async Task<IEnumerable<JokeModel>> GetJokesAsync(
            string language,
            string category,
            CancellationToken cancellationToken)
        {
            var jokes = await this.GetCollectionAsync();

            var collection = new List<JokeModel>();

            using (var transaction = this.manager.CreateTransaction())
            {
                var enumeration = await jokes.CreateEnumerableAsync(transaction, EnumerationMode.Unordered);
                var enumerator = enumeration.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    collection.Add(enumerator.Current.Value);
                }
            }

            return collection;
        }

        public async Task ImportJokesAsync(
            IEnumerable<JokeImportModel> importJokes,
            CancellationToken cancellationToken)
        {
            var jokes = await this.GetCollectionAsync();

            var mapper = this.mapperConfiguration.CreateMapper();

            using (var transaction = this.manager.CreateTransaction())
            {
                foreach (var importJoke in importJokes)
                {
                    var joke = mapper.Map<JokeImportModel, JokeModel>(importJoke);

                    await jokes.AddOrUpdateAsync(
                        transaction,
                        importJoke.Id,
                        joke,
                        (
                            key,
                            existingJoke) => joke,
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                }

                await transaction.CommitAsync();
            }
        }

        private Task<IReliableDictionary2<Guid, JokeModel>> GetCollectionAsync()
        {
            return this.manager.GetOrAddAsync<IReliableDictionary2<Guid, JokeModel>>(COLLECTION_KEY);
        }
    }
}