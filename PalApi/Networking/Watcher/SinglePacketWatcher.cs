using System;
using System.Threading.Tasks;

namespace PalApi.Networking.Watcher
{
    using Mapping;

    public class SinglePacketWatcher<T> : IWatch
            where T : IPacketMap
    {
        public string PacketCommand { get; set; }
        public TaskCompletionSource<T> OnFound { get; set; }
        public Func<T, bool> Validator { get; set; }

        public bool Validate(IPacketMap packet)
        {
            if (!(packet is T))
                return false;

            if (!PacketCommand.Equals(packet.Command, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (Validator != null && !Validator((T)packet))
                return false;

            OnFound.SetResult((T)packet);

            return true;
        }
    }
}
