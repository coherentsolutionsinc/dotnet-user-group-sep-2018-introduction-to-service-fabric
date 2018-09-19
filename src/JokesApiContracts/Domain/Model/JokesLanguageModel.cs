namespace JokesApiContracts.Domain.Model
{
    public class JokesLanguageModel
    {
        public string Name { get; set; }

        public JokesLanguageCategoryModel[] Categories { get; set; }
    }
}