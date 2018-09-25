using System;

namespace JokesWeb.Models
{
    public class HomeViewModel
    {
        public class JokesLanguageModel
        {
            public string Name { get; set; }

            public JokesLanguageCategoryModel[] Categories { get; set; }
        }

        public class JokesLanguageCategoryModel
        {
            public string Name { get; set; }

            public long Count { get; set; }
        }

        public class JokeModel
        {
            public string Name { get; set; }

            public string Author { get; set; }

            public string Content { get; set; }

            public DateTime PublishDate { get; set; }
        }

        public string CurrentLanguage { get; set; }

        public string CurrentCategory { get; set; }

        public int CurrentViewedCount { get; set; }

        public JokesLanguageModel[] Languages { get; set; }

        public JokeModel[] Jokes { get; set; }
    }
}