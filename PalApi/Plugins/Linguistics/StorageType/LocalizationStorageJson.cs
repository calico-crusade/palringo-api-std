using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PalApi.Plugins.Linguistics.StorageType
{
    using Delegates;

    public class LocalizationStorageJson : ILocalizationStorage
    {
        #region Private fields
        private string filePath;
        private List<Localization> cache;
        #endregion

        #region Cache & Error Handling
        public event ExceptionCarrier OnError = delegate { };
        public List<Localization> Cache => cache ?? (cache = GetLocalizations(filePath));
        #endregion

        #region Constructor
        private LocalizationStorageJson(string filePath)
        {
            this.filePath = filePath;
        }
        #endregion

        #region IO
        public List<Localization> GetLocalizations(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<Localization>();

            try
            {
                return JsonConvert.DeserializeObject<List<Localization>>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                OnError(ex, "Localizations failed to load from Json");
                return new List<Localization>();
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(cache, Formatting.Indented));
            }
            catch (Exception ex)
            {
                OnError(ex, "Unable to save localizations.");
            }
        }
        #endregion

        #region Implementation
        public Localization Get(string languageKey, string id)
        {
            return Cache.FirstOrDefault(t => t.LanguageKey == languageKey && t.TextId == id);
        }

        public Localization[] Get(string id)
        {
            return Cache.Where(t => t.TextId == id).ToArray();
        }

        public Localization[] Get()
        {
            return Cache.ToArray();
        }

        public bool LocalizationAdded(Localization local)
        {
            if (Get(local.LanguageKey, local.TextId) != null)
                return false;

            Cache.Add(local);
            Save();
            return true;
        }

        public bool LocalizationAdded(string languageKey, string id, string text)
        {
            return LocalizationAdded(new Localization(languageKey, id, text));
        }

        public bool LocalizationDeleted(Localization local)
        {
            var get = Get(local.LanguageKey, local.TextId);

            if (get == null)
                return true;

            Cache.Remove(get);
            Save();
            return true;
        }

        public bool LocalizationDeleted(string languageKey, string id)
        {
            return LocalizationDeleted(new Localization(languageKey, id, ""));
        }

        public bool LocalizationModified(Localization local)
        {
            var get = Get(local.LanguageKey, local.TextId);

            if (get == null)
                return LocalizationAdded(local);

            get.Text = local.Text;
            Save();
            return true;
        }

        public bool LocalizationModified(string languageKey, string id, string text)
        {
            return LocalizationModified(new Localization(languageKey, id, text));
        }
        #endregion

        #region Creation
        public static ILocalizationStorage Create(string filePath)
        {
            return new LocalizationStorageJson(filePath);
        }
        #endregion
    }
}
