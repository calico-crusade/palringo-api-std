using System.Collections.Generic;
using System.Linq;

namespace PalApi.Networking
{
    public interface IPacket
    {
        string Command { get; set; }
        Dictionary<string, string> Headers { get; set; }
        byte[] Payload { get; set; }
        string Content { get; set; }

        int ContentLength { get; set; }
        long MessageId { get; set; }

        string this[string key] { get; set; }

        IPacket Clone();
    }

    public class Packet : IPacket
    {
        /// <summary>
        /// The command of the packet
        /// </summary>
        public string Command { get; set; } = "";
        /// <summary>
        /// The headers of the packet
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// The payload of the packet
        /// </summary>
        public byte[] Payload { get; set; }

        public string Content
        {
            get { return PacketSerializer.Outbound.GetString(Payload); }
            set { Payload = PacketSerializer.Outbound.GetBytes(value); }
        }

        /// <summary>
        /// Gets or sets the content length of the packet
        /// </summary>
        public int ContentLength
        {
            get
            {
                return Headers.ContainsKey("CONTENT-LENGTH") ?
                    int.Parse(Headers["CONTENT-LENGTH"]) : 0;
            }
            set
            {
                if (Headers.ContainsKey("CONTENT-LENGTH"))
                    Headers["CONTENT-LENGTH"] = value.ToString();
                else
                    Headers.Add("CONTENT-LENGTH", value.ToString());
            }
        }

        /// <summary>
        /// Gets the message id for the packet.
        /// </summary>
        public long MessageId
        {
            get
            {
                return Headers.ContainsKey("MESG-ID") ? int.Parse(Headers["MESG-ID"]) : -1;
            }
            set
            {
                if (Headers.ContainsKey("MESG-ID"))
                {
                    Headers["MESG-ID"] = value.ToString();
                    return;
                }

                Headers.Add("MESG-ID", value.ToString());
            }
        }

        /// <summary>
        /// Access to the packets headers securly without risk of exception
        /// </summary>
        /// <param name="key">The header key to access</param>
        /// <returns>Null if the header doesn't exist, or the header value if it does exist</returns>
        public string this[string key]
        {
            get
            {
                return Headers.ContainsKey(key) ? Headers[key] : null;
            }
            set
            {
                if (Headers.ContainsKey(key))
                    Headers[key] = value;
                else
                    Headers.Add(key, value);
            }
        }

        /// <summary>
        /// Default constructor for the packet
        /// </summary>
        public Packet() { }

        /// <summary>
        /// Printable format of the packet
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = string.Format("{0}{1}{2}", Command,
                Headers.Count > 0 ? "\r\n" + string.Join("\r\n", Headers.Select(t => t.Key + ": " + t.Value).ToList()) : "",
                Payload.Length > 0 ? "\r\n" + System.Text.Encoding.UTF8.GetString(Payload) : "");
            return output;
        }

        public IPacket Clone()
        {
            return new Packet
            {
                Command = Command,
                Headers = Headers.ToDictionary(t => t.Key, t => t.Value),
                Payload = Payload?.ToArray()
            };
        }
    }
}
