using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PalApi.Plugins.Linguistics.StorageType
{
    using Delegates;

    public class LocalizationStorageFlatFile : ILocalizationStorage
    {
        #region private fields
        private string filePath;
        private List<Localization> cache;
        #endregion

        #region Cache & Error Handling
        public event ExceptionCarrier OnError = delegate { };
        public List<Localization> Cache => cache ?? (cache = GetLocalizations(filePath));
        #endregion

        #region construtor
        private LocalizationStorageFlatFile(string filePath)
        {
            this.filePath = filePath;
        }
        #endregion

        #region IO
        public string Serialization(List<Localization> localizations)
        {
            var locals = localizations.OrderBy(t => t.LanguageKey + t.TextId).ToArray();

            var bob = new StringBuilder();

            string lastLang = null;
            foreach(var local in locals)
            {
                if (lastLang == null || lastLang != local.LanguageKey)
                {
                    bob.AppendLine($"[{local.LanguageKey}]");
                    lastLang = local.LanguageKey;
                }

                bob.AppendLine($"{local.TextId} = {local.Text.Replace("\r", "\\r").Replace("\n", "\\n")}");
            }

            return bob.ToString();
        }

        public List<Localization> Deserialization(string content)
        {
            var locals = new List<Localization>();
            string lastLang = null;

            var lines = content.Split(new[] { Environment.NewLine, "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    lastLang = line.Substring(1, line.Length - 2);
                    continue;
                }

                if (string.IsNullOrEmpty(lastLang))
                    continue;

                if (line.StartsWith("##"))
                    continue;

                if (!line.Contains("="))
                    continue;

                var args = line.Split('=');
                var id = args[0].Trim();
                var text = string.Join("=", args.Skip(1).ToArray()).Trim();

                locals.Add(new Localization(lastLang, id, text.Replace("\\r", "\r").Replace("\\n", "\n")));
            }

            return locals;
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(filePath, Serialization(Cache));
            }
            catch (Exception ex)
            {
                OnError(ex, "Error saving localizations to flat file");
            }
        }

        public List<Localization> GetLocalizations(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<Localization>();

            try
            {
                return Deserialization(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                OnError(ex, "Localizations failed to load from Flat File");
                return new List<Localization>();
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
            return new LocalizationStorageFlatFile(filePath);
        }
        #endregion
    }
}
