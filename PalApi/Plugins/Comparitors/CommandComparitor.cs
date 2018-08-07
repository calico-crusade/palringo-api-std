using System;

namespace PalApi.Plugins.Comparitors
{
    public class CommandComparitor : IComparitorProfile
    {
        public Type AttributeType => typeof(Command);

        public bool IsMatch(IPalBot bot, Message msg, string cmd, ICommand atr, out string capped, out Message nm)
        {
            capped = cmd;
            nm = msg;

            if (!cmd.ToLower().StartsWith(atr.Comparitor.ToLower()))
                return false;

            capped = cmd.Remove(0, atr.Comparitor.Length);
            return true;
        }
    }
}
