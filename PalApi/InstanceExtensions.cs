using System;

namespace PalApi
{
    public static class InstanceExtensions
    {
        public static IPalBot UseConsoleLogging(this IPalBot bot, bool logUnhandled = false)
        {
            if (logUnhandled)
                bot.OnUnhandledPacket += (p) =>
                    Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rUNHANDLED ^w{p.Command}");

            bot.OnPacketReceived += (p) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^c<< ^w{p.Command} - ^b{p.ContentLength}");

            bot.OnPacketSent += (p) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^e>> ^w{p.Command} - ^b{p.ContentLength}");

            bot.OnDisconnected += () =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^yDisconnected..");

            bot.OnException += (e, n) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rERROR {n} - {e.ToString()}");

            bot.OnLoginFailed += (r) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rLogin Failed: {r}");

            return bot;
        }
        public static IPalBot AddAuth(this IPalBot bot, params int[] ids)
        {
            Plugins.Defaults.AuthRole.AuthorizedUsers.AddRange(ids);
            return bot;
        }
        public static IPalBot RemoveAuth(this IPalBot bot, params int[] ids)
        {
            foreach (var id in ids)
                Plugins.Defaults.AuthRole.AuthorizedUsers.Remove(id);
            return bot;
        }
        public static IPalBot AutoReconnect(this IPalBot bot)
        {
            bot.OnDisconnected += () => bot.Login(bot.Email, bot.Password, bot.Status, bot.Device, bot.SpamFilter);
            return bot;
        }
    }
}
