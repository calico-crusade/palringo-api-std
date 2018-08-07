using System.Threading.Tasks;

namespace PalApi.Plugins.Defaults
{
    using Types;
    using SubProfile;

    public class AdminRole : IRole
    {
        public string Name => "Admin";

        public void OnRejected(IPalBot bot, Message msg)
        {
            
        }

        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            return (await new OwnerRole().ValidateGm(bot, msg, group, groupUser)) || groupUser.UserRole == Role.Admin;
        }

        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            return false;
        }
    }
}
