namespace PalApi.Plugins.Linguistics
{
    public class LinguisticsEngine
    {
        public ILocalizationStorage Storage { get; private set; }

        public LinguisticsEngine(ILocalizationStorage storage)
        {
            Storage = storage;
        }

        public string this[string lang, string id]
        {
            get { return Storage.Get(lang, id)?.Text; }
            set { Storage.LocalizationModified(lang, id, value); }
        }
    }
}
