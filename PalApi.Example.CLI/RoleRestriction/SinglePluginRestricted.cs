using PalApi.Plugins;
using PalApi.Types;

namespace PalApi.Example.CLI.RoleRestriction
{
    [Command("!test")] //Roles on this command will restrict any plugin in the class
    public class SinglePluginRestricted : IPlugin
    {
        [Command("hello")]
        public async void NotRestricted(IPalBot bot, Message msg, string cmd)
        {
            await bot.Reply(msg, "Anyone can access this command!");
        }

        [Command("restricted", Roles = "Custom, Auth")]
        public async void CustomRoleRestricted(IPalBot bot, Message msg, string cmd)
        {
            await bot.Reply(msg, "Only people who meet the conditions of the \"CustomRole.cs\" role and people who are \"Authorized\" can use this plugin.");

            //Possible roles:
            //Auth - Only people who are Authorized within the bot.
            //Owner - Only Auth and the Owner of the group
            //Admin - Only Owners, Authorized people, and Admins of the group
            //Mod - Only Owners, Authorized people, Admins and Mods of the group
            //See PalApi.Plugins.Defaults for the instances of the roles.
            //Any custom roles you create.
        }

        [Command("pm only", MessageType = MessageType.Private)]
        public async void PrivateMessageOnly(IPalBot bot, Message msg, string cmd)
        {
            await bot.Reply(msg, "This command can only be used in PM");
        }

        [Command("gm only", MessageType = MessageType.Group)]
        public async void GroupMessageOnly(IPalBot bot, Message msg, string cmd)
        {
            await bot.Reply(msg, "This command can only be used in GM");
        }
    }
}
