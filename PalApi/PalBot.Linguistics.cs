using System.Threading.Tasks;

namespace PalApi
{
    using PacketTypes;
    using Plugins.Linguistics;

    public interface IPalBotLinguistics : IPalBotSenders
    {
        Task<Response> Reply(LangMessage msg, string id, params object[] parameters);
        Task<Response> Group(int gid, string language, string id, params object[] parameters);
        Task<Response> Private(int uid, string language, string id, params object[] parameters);
    }

    public partial class PalBot
    {
        private void CheckEngine(string language)
        {
            if (Languages == null)
                throw new LinguisticsEngineNotFound();

            if (string.IsNullOrEmpty(language))
                throw new LanguageNotFound();
        }

        private string CheckLanguage(string language, string id)
        {
            CheckEngine(language);

            var lang = Languages[language, id];

            if (lang == null)
                throw new LanguageNotFound(language + " - " + id);

            return lang;
        }

        public async Task<Response> Reply(LangMessage msg, string id, params object[] parameters)
        {
            var lang = CheckLanguage(msg?.LanguageKey, id);

            if (parameters == null || parameters.Length == 0)
                return await Reply((Message)msg, lang);

            return await Reply((Message)msg, string.Format(lang, parameters));
        }

        public async Task<Response> Group(int gid, string language, string id, params object[] parameters)
        {
            var lang = CheckLanguage(language, id);

            if (parameters == null || parameters.Length == 0)
                return await Group(gid, lang);

            return await Group(gid, string.Format(lang, parameters));
        }

        public async Task<Response> Private(int uid, string language, string id, params object[] parameters)
        {
            var lang = CheckLanguage(language, id);

            if (parameters == null || parameters.Length == 0)
                return await Private(uid, lang);

            return await Private(uid, string.Format(lang, parameters));
        }
    }
}
