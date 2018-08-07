using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalApi.Plugins.Defaults
{
    using SubProfile;

    public class DoucheRole : IRole
    {
        public static List<int> Douches = new List<int>();

        public string Name => "DoucheGaurd";

        public void OnRejected(IPalBot bot, Message msg) { }

        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            return await ValidatePm(bot, msg, groupUser);
        }

        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            return !Douches.Contains(msg.UserId);
        }
    }
}
