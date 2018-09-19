using System;

namespace JokesApiContracts.Domain.Model
{
    public class JokeImportModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }

        public string Category { get; set; }

        public string Language { get; set; }

        public DateTime PublishDate { get; set; }
    }
}