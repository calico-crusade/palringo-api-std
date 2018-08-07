using System;

namespace PalApi.Plugins.Comparitors
{
    public interface IComparitorProfile
    {
        Type AttributeType { get; }

        bool IsMatch(IPalBot bot, Message msg, string cmd, ICommand atr, out string capped, out Message newMessage);
    }
}
