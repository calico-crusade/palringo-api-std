namespace PalApi.Plugins.Linguistics
{
    public class Localization
    {
        public string LanguageKey { get; set; }
        public string TextId { get; set; }
        public string Text { get; set; }

        public Localization() { }

        public Localization(string lang, string id, string text)
        {
            LanguageKey = lang;
            TextId = id;
            Text = text;
        }
    }
}
