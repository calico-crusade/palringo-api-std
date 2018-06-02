using System;
using System.Collections.Generic;
using System.Reflection;

namespace PalApi.Networking.Mapping
{
    using Utilities;

    public interface IPacketMapper
    {
        IPacketMap Map(IPacket packet);
        IPacket Unmap(IPacketMap map);
    }

    public class PacketMapper : IPacketMapper
    {
        private static Dictionary<string, MappedPacket> PacketMaps { get; set; }

        private IReflectionUtility reflection;

        public PacketMapper(IReflectionUtility reflection)
        {
            this.reflection = reflection;
        }
        
        public IPacket Unmap(IPacketMap map)
        {
            if (!PacketMaps.ContainsKey(map.Command))
                return null; //No packet mappers for type

            var mapper = PacketMaps[map.Command];

            if (mapper.Payloads != null && mapper.Payloads.Count > 1)
                return null; //Cannot map a packet that has multiple payload accessors.
            
            var packet = new Packet
            {
                Command = map.Command,
                Headers = new Dictionary<string, string>()
            };

            foreach(var header in mapper.Headers)
            {
                packet[header.Key] = header.Value.GetValue(map).ToString();
            }

            if (mapper.Payloads != null)
            {
                packet.Payload = reflection.ChangeType<byte[]>(mapper.Payloads[0].GetValue(map));
            }

            return packet;
        }

        public IPacketMap Map(IPacket packet)
        {
            if (PacketMaps == null)
                LoadPacketMaps();
            
            if (!PacketMaps.ContainsKey(packet.Command))
                return null;

            var mapper = PacketMaps[packet.Command];

            var instance = Activator.CreateInstance(mapper.PacketType);

            foreach(var header in mapper.Headers)
            {
                if (!packet.Headers.ContainsKey(header.Key))
                    continue;

                SetProperty(instance, header.Value, packet[header.Key]);
            }

            foreach(var payload in mapper.Payloads)
            {
                SetProperty(instance, payload, packet.Payload ?? new byte[0]);
            }

            return (IPacketMap)instance;
        }

        private void SetProperty(object item, PropertyInfo prop, object value)
        {
            try
            {
                prop.SetValue(item, reflection.ChangeType(value, prop.PropertyType), null);
            }
            catch { }
        }

        private void LoadPacketMaps()
        {
            var packetTypes = reflection.GetTypes(typeof(IPacketMap));

            PacketMaps = new Dictionary<string, MappedPacket>();

            foreach (var pack in packetTypes)
            {
                var instance = (IPacketMap)Activator.CreateInstance(pack);

                if (PacketMaps.ContainsKey(instance.Command))
                    continue;

                var headers = new Dictionary<string, PropertyInfo>();
                var payloads = new List<PropertyInfo>();

                var props = pack.GetProperties();

                foreach (var prop in props)
                {
                    if (Attribute.IsDefined(prop, typeof(Header)))
                    {
                        var header = prop.GetCustomAttribute<Header>();
                        if (!headers.ContainsKey(header.Name))
                            headers.Add(header.Name, prop);
                    }

                    if (!Attribute.IsDefined(prop, typeof(Payload)))
                        continue;

                    payloads.Add(prop);
                }

                PacketMaps.Add(instance.Command, new MappedPacket(pack, headers, payloads));
            }
        }

        private class MappedPacket
        {
            public Type PacketType { get; set; }
            public Dictionary<string, PropertyInfo> Headers { get; set; }
            public List<PropertyInfo> Payloads { get; set; }

            public MappedPacket(Type packetType, Dictionary<string, PropertyInfo> headers, List<PropertyInfo> payloads)
            {
                this.PacketType = packetType;
                this.Headers = headers;
                this.Payloads = payloads;
            }
        }
    }
}
