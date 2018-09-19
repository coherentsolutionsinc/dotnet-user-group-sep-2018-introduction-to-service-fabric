using System;

namespace JokesApiContracts.Domain.Model
{
    public class JokeModel
    {
        public string Name { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }

        public DateTime PublishDate { get; set; }
    }
}