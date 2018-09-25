using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using AutoMapper;

using JokesApiContracts.Domain.Model;

using JokesWeb.Clients;
using JokesWeb.Models;

using Microsoft.AspNetCore.Mvc;

namespace JokesWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly MapperConfiguration mappingConfig;

        private readonly IJokesApiClient jokesApiClient;

        private readonly IJokesStatsClient jokesStatsClient;

        public HomeController(
            IJokesApiClient jokesApiClient,
            IJokesStatsClient jokesStatsClient)
        {
            this.mappingConfig = new MapperConfiguration(config => config.CreateMap<JokesLanguageCategoryModel, HomeViewModel.JokesLanguageCategoryModel>());
            this.mappingConfig = new MapperConfiguration(config => config.CreateMap<JokesLanguageModel, HomeViewModel.JokesLanguageModel>());
            this.mappingConfig = new MapperConfiguration(config => config.CreateMap<JokeModel, HomeViewModel.JokeModel>());

            this.jokesApiClient = jokesApiClient;
            this.jokesStatsClient = jokesStatsClient;
        }

        public async Task<IActionResult> Index(
            [FromQuery] string language,
            [FromQuery] string category)
        {
            var languages = await this.jokesApiClient.GetLanguagesAsync(this.HttpContext.RequestAborted);

            var stats = await this.jokesStatsClient.IncrementViewedAndGetStatisticsAsync(
                this.GetUser(),
                language,
                category,
                this.HttpContext.RequestAborted);

            var jokes = string.IsNullOrEmpty(language) || string.IsNullOrEmpty(category)
                ? Array.Empty<JokeModel>()
                : await this.jokesApiClient.GetJokesAsync(language, category, this.HttpContext.RequestAborted);

            var mapper = this.mappingConfig.CreateMapper();
            return this.View(
                new HomeViewModel
                {
                    CurrentLanguage = language,
                    CurrentCategory = category,
                    CurrentViewedCount = stats.ViewedCount,
                    Languages = mapper.Map<IEnumerable<JokesLanguageModel>, HomeViewModel.JokesLanguageModel[]>(languages),
                    Jokes = mapper.Map<IEnumerable<JokeModel>, HomeViewModel.JokeModel[]>(jokes)
                });
        }

        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        private string GetUser()
        {
            return this.HttpContext.Items["user-cookie"].ToString();
        }
    }
}