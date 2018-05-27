using System;
using System.Threading.Tasks;

namespace PalApi.Networking.Watcher
{
    using Mapping;

    public class SinglePacketWatchWithCancel<T, T2> : IWatch
        where T: IPacketMap
        where T2: IPacketMap
    {
        public TaskCompletionSource<T> OnFound { get; set; }
        public Func<T, bool> Validator { get; set; }
        public Func<T2, bool> CancelValidator { get; set; }

        public bool Validate(IPacketMap packet)
        {
            if (CheckCanel(packet))
                return true;

            if (!(packet is T))
                return false;

            if (Validator != null && !Validator((T)packet))
                return false;

            OnFound.SetResult((T)packet);
            return true;
        }

        private bool CheckCanel(IPacketMap packet)
        {
            if (!(packet is T2))
                return false;

            if (CancelValidator != null && !CancelValidator((T2)packet))
                return false;

            OnFound.SetException(new PacketException(packet));
            return true;
        }
    }
}
