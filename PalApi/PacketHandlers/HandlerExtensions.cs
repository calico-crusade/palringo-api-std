namespace PalApi.PacketHandlers
{
    using Types;

    public static class HandlerExtensions
    {
        public static Role ToRole(this AdminActions action)
        {
            switch (action)
            {
                case AdminActions.Admin:
                    return Role.Admin;
                case AdminActions.Mod:
                    return Role.Mod;
                case AdminActions.Kick:
                    return Role.NotMember;
                case AdminActions.Ban:
                    return Role.Banned;
                case AdminActions.Silence:
                    return Role.Silenced;
                default:
                    return Role.User;
            }
        }
    }
}
