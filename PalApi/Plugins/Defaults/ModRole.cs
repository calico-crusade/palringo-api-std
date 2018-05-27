using System.Threading.Tasks;

namespace PalApi.Plugins.Defaults
{
    using SubProfile;
    using Types;

    public class ModRole : IRole
    {
        public string Name => "Mod";

        public void OnRejected(IPalBot bot, Message msg)
        {
            
        }

        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            return (await new AdminRole().ValidateGm(bot, msg, group, groupUser)) || groupUser.UserRole == Role.Mod;
        }

        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            return false;
        }
    }
}
