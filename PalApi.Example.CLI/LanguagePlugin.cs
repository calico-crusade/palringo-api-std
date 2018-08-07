namespace PalApi.Example.CLI
{
    using Plugins;
    using Plugins.Linguistics;

    [LCommand("plugin")] //Make sure to use LCommand and not Command when using multi-language support
    public class LanguagePlugin : IPlugin
    {
        [LCommand("plugin.command")] //This relates to the localization.lang file in the root of the CLI project
        public async void Test(IPalBot bot, LangMessage msg, string cmd)
        {
            await bot.Reply(msg, "plugin.response"); 
            //This is an example of how to use the parameters localization files.
            await bot.Reply(msg, "plugin.cant", (await bot.GetUser(msg.UserId)).Nickname, msg.LanguageKey);
        }
    }
}
