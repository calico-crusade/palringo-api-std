using System;

namespace PalApi
{
    public static class InstanceExtensions
    {
        public static IPalBot UseConsoleLogging(this IPalBot bot, bool logUnhandled = false)
        {
            if (logUnhandled)
                bot.On.UnhandledPacket += (p) =>
                    Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rUNHANDLED ^w{p.Command}");

            bot.On.PacketReceived += (p) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^c<< ^w{p.Command} - ^b{p.ContentLength}");

            bot.On.PacketSent += (p) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^e>> ^w{p.Command} - ^b{p.ContentLength}");

            bot.On.Disconnected += () =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^yDisconnected..");

            bot.On.Exception += (e, n) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rERROR {n} - {e.ToString()}");

            bot.On.LoginFailed += (b, r) =>
                Extensions.ColouredConsole($"^w[^g{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}^w] ^rLogin Failed: {r.Reason}");

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
        public static IPalBot AddDouche(this IPalBot bot, params int[] ids)
        {
            Plugins.Defaults.DoucheRole.Douches.AddRange(ids);
            return bot;
        }
        public static IPalBot RemoveDouche(this IPalBot bot, params int[] ids)
        {
            foreach (var id in ids)
                Plugins.Defaults.DoucheRole.Douches.Remove(id);
            return bot;
        }
        public static IPalBot AutoReconnect(this IPalBot bot)
        {
            bot.On.Disconnected += async () => await bot.Login(bot.Email, bot.Password, bot.Status, bot.Device, bot.SpamFilter);
            return bot;
        }
        public static IPalBot ReloginOnThrottle(this IPalBot bot)
        {
            bot.On.Throttle += async (b, c) => await b.Login(b.Email, b.Password, b.Status, b.Device, b.SpamFilter, b.EnablePlugins);
            return bot;
        }
    }
}
