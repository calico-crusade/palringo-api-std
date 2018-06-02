using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalApi.Networking
{
    using Delegates;

    public interface IPacketDeserializer
    {
        event NetworkPacketCarrier OnPacketParsed;
        event ExceptionCarrier OnException;

        void ReadPacket(INetworkClient client, byte[] data);
    }

    public class PacketDeserializer : IPacketDeserializer
    {
        public static Encoding Display => PacketSerializer.Outbound;
        public static Encoding Outbound => PacketSerializer.Outbound;
        public static byte[] NewLine => new byte[] { (byte)'\r', (byte)'\n' };
        public static byte HeaderCharacter => (byte)':';

        public event NetworkPacketCarrier OnPacketParsed = delegate { };
        public event ExceptionCarrier OnException = delegate { };
        
        private byte[] overflow;

        public void ReadPacket(INetworkClient client, byte[] prestine)
        {
            try
            {
                byte[] data;

                if (overflow == null || overflow.Length <= 0)
                {
                    data = new byte[prestine.Length];
                    Array.Copy(prestine, data, data.Length);
                }
                else
                {
                    data = new byte[prestine.Length + overflow.Length];
                    Array.Copy(overflow, data, overflow.Length);
                    Array.Copy(prestine, 0, data, overflow.Length, prestine.Length);
                }

                string packetString = Outbound.GetString(data);

                var cmd = data.FirstInstanceOf(NewLine).ToArray();
                data = data.Skip(cmd.Length + NewLine.Length).ToArray();

                var pack = new Packet
                {
                    Command = Outbound.GetString(cmd),
                    Headers = new Dictionary<string, string>()
                };

                if (data.Length <= 0)
                {
                    OnPacketParsed(client, pack);
                    overflow = null;
                    return;
                }
                
                while (true)
                {
                    var part = data.FirstInstanceOf(NewLine).ToArray();
                    if (part.Length <= 0)
                        break;

                    data = data.Skip(part.Length + NewLine.Length).ToArray();

                    var key = part.FirstInstanceOf(HeaderCharacter).ToArray();

                    var val = part.Skip(key.Length + 1).ToArray();
                    
                    pack[Outbound.GetString(key).ToUpper()] = Outbound.GetString(val).Trim();
                }

                if (data.Length >= 2 && data[0] == NewLine[0] && data[1] == NewLine[1])
                    data = data.Skip(2).ToArray();

                if (pack.ContentLength == 0)
                {
                    OnPacketParsed(client, pack);
                    overflow = data.Length > 0 ? data : null;
                    return;
                }

                if (pack.ContentLength == data.Length)
                {
                    pack.Payload = data;
                    OnPacketParsed(client, pack);
                    overflow = null;
                    return;
                }

                if (pack.ContentLength > data.Length)
                {
                    if (overflow != null && overflow.Length > 0)
                    {
                        data = new byte[prestine.Length + overflow.Length];
                        Array.Copy(overflow, data, overflow.Length);
                        Array.Copy(prestine, 0, data, overflow.Length, prestine.Length);
                        prestine = data;
                    }
                    overflow = prestine;
                    return;
                }

                pack.Payload = data.Take(pack.ContentLength).ToArray();
                overflow = data.Skip(pack.ContentLength).ToArray();
                OnPacketParsed(client, pack);
            }
            catch (Exception ex)
            {
                OnException(ex, "Error parsing packet");
            }
        }

        private Dictionary<string, string> DeserializeHeaders(string headers)
        {
            var dic = new Dictionary<string, string>();

            var lines = headers.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines)
            {
                if (!line.Contains(':'))
                    continue;

                var key = line.Split(':')[0];
                var val = line.Substring(key.Length + 1).Trim();
                dic.Add(key, val);
            }

            return dic;
        }
    }
} 
