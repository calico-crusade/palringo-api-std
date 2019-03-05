using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PalApi.Networking.Handling
{
    using Mapping;
    using Utilities;

    public interface IPacketHandlerHub
    {
        void ProcessPacket(PalBot bot, IPacketMap packet);
    }

    public class PacketHandlerHub : IPacketHandlerHub
    {
        private IReflectionUtility reflection;
        private List<PacketHandlers> packetHandlers;
        private BroadcastUtility broadcast;

        public PacketHandlerHub(IReflectionUtility reflection, IBroadcastUtility broadcast)
        {
            this.reflection = reflection;
            this.broadcast = (BroadcastUtility)broadcast;
        }

        private void LoadHandlers()
        {
            packetHandlers = new List<PacketHandlers>();

            var handlers = reflection.GetTypes(typeof(IPacketHandler))
                                     .Select(t => (IPacketHandler)reflection.GetInstance(t))
                                     .ToArray();

            foreach (var handler in handlers)
            {
                var hndlr = new PacketHandlers
                {
                    Instance = handler,
                    Handlers = new Dictionary<string, MethodInfo>()
                };

                var methods = handler.GetType().GetMethods();

                foreach(var method in methods)
                {
                    var args = method.GetParameters();

                    if (args.Length < 1)
                        continue;

                    var first = args.First();

                    if (!first.ParameterType.GetTypeInfo().ImplementedInterfaces.Any(t => t == typeof(IPacketMap)))
                        continue;

                    if (first.ParameterType.IsAbstract || first.ParameterType.IsInterface)
                        continue;

                    var packet = (IPacketMap)Activator.CreateInstance(first.ParameterType);
                    if (hndlr.Handlers.ContainsKey(packet.Command.ToUpper()))
                    {
                        broadcast.BroadcastException(new Exception(), $"Handler already exists in {method.Name} for {packet.Command}");
                        continue;
                    }

                    hndlr.Handlers.Add(packet.Command.ToUpper(), method);
                }

                packetHandlers.Add(hndlr);
            }
        }

        public void ProcessPacket(PalBot bot, IPacketMap packet)
        {
            if (packetHandlers == null)
                LoadHandlers();

            var handlers = packetHandlers.Where(t => t.Handlers.ContainsKey(packet.Command.ToUpper()));

            foreach (var handler in handlers)
            {
                var method = handler.Handlers[packet.Command.ToUpper()];

                try
                {
                    method.Invoke(handler.Instance, new object[] { packet, bot });
                }
                catch (Exception ex)
                {
                    broadcast.BroadcastException(ex, $"Error processing handler {method.Name} for {packet.Command.ToUpper()}");
                }
            }

        }

        private class PacketHandlers
        {
            public IPacketHandler Instance { get; set; }
            public Dictionary<string, MethodInfo> Handlers { get; set; }
        }
    }
}
