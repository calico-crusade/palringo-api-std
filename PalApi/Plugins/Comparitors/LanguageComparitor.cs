using System;
using System.Linq;

namespace PalApi.Plugins.Comparitors
{
    using Linguistics;

    public class LanguageComparitor : IComparitorProfile
    {
        public Type AttributeType => typeof(LCommand);

        public bool IsMatch(IPalBot bot, Message msg, string cmd, ICommand atr, out string capped, out Message nm)
        {
            nm = msg;
            if (msg is LangMessage && !string.IsNullOrEmpty(((LangMessage)msg).LanguageKey))
                return IsMatchAlready(bot, (LangMessage)msg, cmd, atr, out capped);

            capped = cmd;

            var localizations = bot.Languages.Storage.Get(atr.Comparitor);
            if (localizations.Length <= 0)
                return false;

            var local = localizations.FirstOrDefault(t => cmd.ToLower().StartsWith(t.Text.ToLower()));

            if (local == null)
                return false;

            nm = new LangMessage(msg, local.LanguageKey);
            capped = cmd.Remove(0, local.Text.Length);
            return true;
        }

        private bool IsMatchAlready(IPalBot bot, LangMessage msg, string cmd, ICommand atr, out string capped)
        {
            capped = cmd;
            var local = bot.Languages[msg.LanguageKey, atr.Comparitor];

            if (local == null)
                return false;

            if (!cmd.ToLower().StartsWith(local.ToLower()))
                return false;

            capped = cmd.Remove(0, local.Length);
            return true;
        }
    }
}
