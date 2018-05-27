using System.Threading.Tasks;

namespace PalApi.Plugins
{
    using SubProfile;

    public interface IRole
    {
        string Name { get; }

        Task<bool> ValidatePm(IPalBot bot, Message msg, User user);
        Task<bool> ValidateGm(IPalBot bot, Message msg, Group group, GroupUser groupUser);

        void OnRejected(IPalBot bot, Message msg);
    }
}
