using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalApi.Networking
{
    using Delegates;

    public interface IPacketSerializer
    {
        event ExceptionCarrier OnException;

        IEnumerable<byte[]> Serialize(IPacket packet);
    }

    public class PacketSerializer : IPacketSerializer
    {
        public event ExceptionCarrier OnException = delegate { };

        public const int MaxPayloadSize = 512;
        public static Encoding Outbound = Encoding.UTF8;
        
        private long messageId = 1;

        public IEnumerable<byte[]> Serialize(IPacket packet)
        {
            if (packet.Payload == null || packet.Payload.Length <= MaxPayloadSize)
            {
                yield return SerializePacket(SinglePacket(packet), out long msgid);
                packet.MessageId = msgid;
                yield break;
            }

            var broken = packet.Payload.SplitChunks(MaxPayloadSize).ToArray();
            
            yield return SerializePacket(HeadPacket(packet, broken.First(), packet.Payload.Length), out long corid);
            packet.MessageId = corid;

            for (var i = 1; i < broken.Length - 1; i++)
            {
                yield return SerializePacket(MidPacket(packet, broken[i], corid), out long msgid);
            }

            yield return SerializePacket(LastPacket(packet, broken.Last(), corid), out long msgId);
        }

        private byte[] SerializePacket(IPacket packet, out long msgId)
        {
            try
            {
                msgId = messageId;
                packet.MessageId = msgId;
                messageId++;

                if (packet.Payload != null && packet.Payload.Length > 0)
                {
                    packet.ContentLength = packet.Payload.Length;
                }

                var headers = string.Join("", packet.Headers.Select(t => $"{t.Key}: {t.Value}\r\n"));

                var cap = $"{packet.Command}\r\n{headers}\r\n";

                var topBytes = Outbound.GetBytes(cap);

                return topBytes.Extend(packet.Payload ?? new byte[0]).ToArray();
            }
            catch (Exception ex)
            {
                OnException(ex, "Error serializing packet");
                msgId = -1;
                return new byte[0];
            }
        }

        private IPacket HeadPacket(IPacket packet, byte[] chunk, int totalLength)
        {
            var p = packet.Clone();
            p["TOTAL-LENGTH"] = totalLength.ToString();
            p.Payload = chunk;
            return p;
        }

        private IPacket MidPacket(IPacket packet, byte[] chunk, long corid)
        {
            var p = packet.Clone();
            p["CORRELATION-ID"] = corid.ToString();
            p.Payload = chunk;
            return p;
        } 

        private IPacket LastPacket(IPacket packet, byte[] chunk, long corid)
        {
            var p = MidPacket(packet, chunk, corid);
            p["LAST"] = "1";
            return p;
        }

        private IPacket SinglePacket(IPacket packet)
        {
            var p = packet.Clone();
            p["LAST"] = "T";
            p.Payload = packet.Payload;
            return p;
        }
    }
}
