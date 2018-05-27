using System.Linq;
using System.Threading.Tasks;

namespace PalApi.Plugins
{
    using Types;
    using Utilities;

    public interface IRoleManager
    {
        Task<bool> IsInRole(string role, IPalBot bot, Message msg);
    }

    public class RoleManager : IRoleManager
    {
        private IRole[] Roles;

        private IReflectionUtility reflection;
        public RoleManager(IReflectionUtility reflection)
        {
            this.reflection = reflection;
        }

        private void LoadRoles()
        {
            Roles = reflection
                .GetTypes(typeof(IRole))
                .Select(t => (IRole)reflection.GetInstance(t))
                .ToArray();
        }

        public async Task<bool> IsInRole(string role, IPalBot bot, Message msg)
        {
            if (Roles == null)
                LoadRoles();

            var roles = role.Split(',').Select(t => t.Trim().ToLower()).ToArray();
            foreach(var r in Roles)
            {
                if (!roles.Any(t => t == r.Name.ToLower().Trim()))
                    continue;

                if (msg.MesgType == MessageType.Group && !(await IsInGroupRole(r, bot, msg)))
                    return false;

                if (msg.MesgType == MessageType.Private && !(await IsInPrivateRole(r, bot, msg)))
                    return false;
            }

            return true;
        }

        private async Task<bool> IsInGroupRole(IRole role, IPalBot bot, Message msg)
        {
            var group = bot.GetGroup(msg.ReturnAddress);

            if (group == null)
                return false;

            var members = bot.GetGroupMembers(group.Id).ToArray();

            if (members == null)
                return false;

            var gu = members.Where(t => t.Id == msg.UserId).FirstOrDefault();
            if (gu == null)
                return false;

            if (!(await role.ValidateGm(bot, msg, group, gu)))
            {
                role.OnRejected(bot, msg);
                return false;
            }

            return true;
        }

        private async Task<bool> IsInPrivateRole(IRole role, IPalBot bot, Message msg)
        {
            var user = await bot.GetUser(msg.UserId);

            if (!(await role.ValidatePm(bot, msg, user)))
            {
                role.OnRejected(bot, msg);
                return false;
            }

            return true;
        }
    }
}
