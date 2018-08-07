using System.Threading.Tasks;

namespace PalApi.Plugins.Defaults
{
    using SubProfile;
    using Types;

    public class OwnerRole : IRole
    {
        public string Name => "Owner";

        public void OnRejected(IPalBot bot, Message msg)
        {
            
        }

        public async Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser)
        {
            return (await new AuthRole().ValidateGm(bot, msg, group, groupUser)) || groupUser.UserRole == Role.Owner;
        }

        public async Task<bool> ValidatePm(IPalBot bot, Message msg, User user)
        {
            return false;
        }
    }
}
