using System;
using System.Threading.Tasks;

namespace PalApi.Networking.Watcher
{
    using Mapping;

    public class MultiPacketWatchWithCancel : IWatch
    {
        public string[] Watchers { get; set; }
        public string[] Cancelrs { get; set; }

        public TaskCompletionSource<IPacketMap> OnFound { get; set; }

        public bool Validate(IPacketMap packet)
        {
            foreach(var watch in Watchers ?? new string[0])
            {
                if (watch != packet.Command)
                    continue;

                OnFound.SetResult(packet);
                return true;
            }

            foreach(var cancel in Cancelrs ?? new string[0])
            {
                if (cancel != packet.Command)
                    continue;

                OnFound.SetException(new PacketException(packet));
                return true;
            }

            return false;
        }
    }

    public class PacketException : Exception
    {
        public IPacketMap Packet { get; set; }

        public PacketException(IPacketMap map)
        {
            Packet = map;
        }

    }
}
