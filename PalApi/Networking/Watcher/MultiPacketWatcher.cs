using System.Threading.Tasks;

namespace PalApi.Networking.Watcher
{
    using Mapping;

    public class MultiPacketWatcher : IWatch
    {
        public PacketResponse[] Watchers { get; set; }

        public TaskCompletionSource<PacketResponse> OnFound { get; set; }

        public bool Validate(IPacketMap packet)
        {
            foreach(var watch in Watchers ?? new PacketResponse[0])
            {
                if (watch.Command == packet.Command)
                {
                    watch.Packet = packet;
                    OnFound.SetResult(watch);
                    return true;
                }
            }

            return false;
        }
    }

    public class PacketResponse
    {
        public string Command { get; set; }
        public IPacketMap Packet { get; set; }
    }
}
