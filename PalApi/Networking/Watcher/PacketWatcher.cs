using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PalApi.Networking.Watcher
{
    using Mapping;
    using Utilities;

    public interface IPacketWatcher
    {
        void Process(IPacketMap map);
        Task<T> Subscribe<T>(Func<T, bool> validator) where T : IPacketMap;
        Task<T> Subscribe<T>() where T : IPacketMap;
        Task<T> Subscribe<T, T2>() where T : IPacketMap where T2 : IPacketMap;
        Task<T> Subscribe<T, T2>(Func<T2, bool> cancel) where T : IPacketMap where T2 : IPacketMap;
        Task<T> Subscribe<T, T2>(Func<T, bool> validator) where T : IPacketMap where T2 : IPacketMap;
        Task<T> Subscribe<T, T2>(Func<T, bool> validator, Func<T2, bool> cancel) where T : IPacketMap where T2 : IPacketMap;
        Task<PacketResponse> Subscribe(params IPacketMap[] maps);
        Task<PacketResponse> Subscribe(params string[] maps);
    }

    public class PacketWatcher : IPacketWatcher
    {
        private BroadcastUtility broadcast;

        public PacketWatcher(IBroadcastUtility broadcast)
        {
            this.broadcast = (BroadcastUtility)broadcast;
        }

        private List<IWatch> watches = new List<IWatch>();
        
        public async Task<T> Subscribe<T, T2>()
            where T : IPacketMap
            where T2 : IPacketMap
        {
            return await Subscribe<T, T2>((t) => true, (t) => true);
        }

        public async Task<T> Subscribe<T, T2>(Func<T2, bool> validator)
            where T : IPacketMap
            where T2 : IPacketMap
        {
            return await Subscribe<T, T2>((t) => true, validator);
        }

        public async Task<T> Subscribe<T, T2>(Func<T, bool> validator) 
            where T : IPacketMap 
            where T2 : IPacketMap
        {
            return await Subscribe<T, T2>(validator, (t) => true);
        }

        public async Task<T> Subscribe<T, T2>(Func<T, bool> validator, Func<T2, bool> cancel)
            where T: IPacketMap
            where T2: IPacketMap
        {
            var tsc = new TaskCompletionSource<T>();
            var watch = new SinglePacketWatchWithCancel<T, T2>
            {
                Validator = validator,
                CancelValidator = cancel,
                OnFound = tsc
            };

            watches.Add(watch);

            return await tsc.Task;
        }

        public async Task<T> Subscribe<T>()
            where T: IPacketMap
        {
            return await Subscribe<T>((t) => true);
        }

        public async Task<T> Subscribe<T>(Func<T, bool> validator)
            where T: IPacketMap
        {
            var tsc = new TaskCompletionSource<T>();
            var watch = new SinglePacketWatcher<T>
            {
                PacketCommand = Activator.CreateInstance<T>().Command,
                OnFound = tsc,
                Validator = validator
            };

            watches.Add(watch);

            return await tsc.Task;
        }

        public async Task<PacketResponse> Subscribe(params IPacketMap[] maps)
        {
            return await Subscribe(maps.Select(t => t.Command).ToArray());
        }

        public async Task<PacketResponse> Subscribe(params string[] maps)
        {
            var tsc = new TaskCompletionSource<PacketResponse>();
            var watch = new MultiPacketWatcher
            {
                Watchers = maps.Select(t => new PacketResponse
                {
                    Command = t
                }).ToArray(),
                OnFound = tsc
            };

            watches.Add(watch);

            return await tsc.Task;
        }

        public void Process(IPacketMap map)
        {
            foreach(var watch in watches.ToArray())
            {
                try
                {
                    if (watch.Validate(map))
                        watches.Remove(watch);
                }
                catch (Exception ex)
                {
                    broadcast.BroadcastException(ex, "Error running validator for " + map.Command);
                }
            }
        }
    }
}
