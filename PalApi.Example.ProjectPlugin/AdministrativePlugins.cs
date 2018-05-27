using PalApi.Plugins;
using PalApi.Types;

namespace PalApi.Example.ProjectPlugin
{
    [Command("$")]
    public class AdministrativePlugins : IPlugin
    {
        [Command("perma silence", Roles = "Mod", MessageType = MessageType.Group)]
        public async void PermaSilence(IPalBot bot, Message msg, string cmd)
        {
            if (!int.TryParse(cmd, out int userid))
            {
                await bot.Reply(msg, "Who is the user you want to silence? (User Id)");

                var resp = await bot.NextMessage(msg);
                if (!int.TryParse(resp.Content.Trim(), out userid))
                {
                    await bot.Reply(msg, $"\"{resp.Content}\" is not a valid user id. Please try again.");
                    return;
                }
            }

            var user = await bot.GetUser(userid);

            await bot.Reply(msg, $"Ok! I will silence {user.Nickname} whenever they speak! Say \"stop silencing\" to stop the bot!");

            var actionResp = await bot.AdminAction(AdminActions.Silence, userid, msg.GroupId.Value);

            if (actionResp.Code != Code.OK)
            {
                await bot.Reply(msg, $"Does not look like the admin action was successful for some reason?\r\nResponse: {actionResp.ToString()}");
                return;
            }

            while (true)
            {
                var userMessage = await bot.NextMessage(m => m.UserId == userid || m.UserId == msg.UserId);
                if (userMessage.UserId == msg.UserId && msg.Content.ToLower().Trim() == "stop silencing")
                    return;

                if (userMessage.UserId == userid)
                    await bot.AdminAction(AdminActions.Silence, userid, msg.GroupId.Value);
            }
        }
    }
}
