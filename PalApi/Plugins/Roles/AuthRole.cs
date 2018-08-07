using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalApi.Plugins.Defaults
{
    using SubProfile;

    public class AuthRole : IRole
    {
        public static List<int> AuthorizedUsers = new List<int>();

        public string Name => "Auth";

        public void OnRejected(IPalBot bot, Message msg) { }

        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            return await ValidatePm(bot, msg, groupUser);
        }

        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            return AuthorizedUsers.Contains(user.Id);
        }
    }
}
