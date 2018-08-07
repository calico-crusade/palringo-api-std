using System;

namespace PalApi.Plugins.Linguistics
{
    public class LanguageNotFound : Exception
    {
        public LanguageNotFound() : base("Language is null or empty") { }

        public LanguageNotFound(string language) : base("Language " + language + " could not be found.") { }
    }
}
