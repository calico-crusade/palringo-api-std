namespace PalApi.Plugins.Linguistics
{
    using Delegates;

    public interface ILocalizationStorage
    {
        event ExceptionCarrier OnError;

        bool LocalizationAdded(Localization local);
        bool LocalizationAdded(string languageKey, string id, string text);

        bool LocalizationModified(Localization local);
        bool LocalizationModified(string languageKey, string id, string text);

        bool LocalizationDeleted(Localization local);
        bool LocalizationDeleted(string languageKey, string id);

        Localization Get(string languageKey, string id);
        Localization[] Get(string id);
        Localization[] Get();
    }
}
