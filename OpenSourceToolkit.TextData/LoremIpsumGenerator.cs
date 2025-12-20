using Bogus;
using System.Collections.Generic;

namespace OpenSourceToolkit.TextData
{
    public class LoremIpsumGenerator
    {
        private readonly Faker _faker;

        public LoremIpsumGenerator(string locale = "en")
        {
            _faker = new Faker(locale);
        }

        public string GenerateWords(int count)
        {
            return string.Join(" ", _faker.Lorem.Words(count));
        }

        public string GenerateSentences(int count)
        {
            return _faker.Lorem.Sentences(count);
        }

        public string GenerateParagraphs(int count)
        {
            return _faker.Lorem.Paragraphs(count);
        }
    }
}
