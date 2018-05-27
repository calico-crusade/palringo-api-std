using System.Threading.Tasks;
using PalApi.Plugins;
using PalApi.SubProfile;

namespace PalApi.Example.CLI.RoleRestriction
{ 
    /// <summary>
    /// Create a custom role for authorizing plugins
    /// </summary>
    public class CustomRole : IRole
    {
        /// <summary>
        /// The name to be used in the attribute
        /// </summary>
        public string Name => "Custom";

        /// <summary>
        /// What to do when someone fails authorization
        /// </summary>
        /// <param name="bot">The bot that was triggered</param>
        /// <param name="msg">The message that triggered the plugin</param>
        public async void OnRejected(IPalBot bot, Message msg)
        {
            await bot.Reply(msg, "Sorry, you don't have access to use this command");
        }

        /// <summary>
        /// Called when attempting to authorize a group plugin call
        /// </summary>
        /// <param name="bot">The bot that was triggered</param>
        /// <param name="msg">The message that triggered the plugin</param>
        /// <param name="group">The group the plugin was triggered in</param>
        /// <param name="groupUser">The user in the group who triggered the plugin</param>
        /// <returns>Whether or not the user can activate the plugin</returns>
        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            var profile = await bot.GetUser(msg.UserId);

            if (profile.HasTag || profile.Premium)
                return true;

            return false;
        }

        /// <summary>
        /// Called when attempting to authorize a private plugin call
        /// </summary>
        /// <param name="bot">The bot that was triggered</param>
        /// <param name="msg">The message that triggered the plugin</param>
        /// <param name="user">The user who triggered the plugin</param>
        /// <returns>Whether or not the user can activate the plugin</returns>
        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            var profile = await bot.GetUser(msg.UserId);

            if (profile.HasTag || profile.Premium)
                return true;

            return false;
        }
    }
}
