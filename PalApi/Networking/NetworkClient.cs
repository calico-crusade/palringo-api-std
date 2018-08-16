using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PalApi.Networking
{
    using Delegates;
    using System.IO;

    public interface INetworkClient
    {
        event ExceptionCarrier OnException;
        event NetworkClientCarrier OnDisconnected;
        event NetworkDataCarrier OnDataReceived;

        bool Connected { get; }
        string IPAddress { get; }
        string Host { get; }
        int Port { get; }

        Task<bool> Start();
        void Stop();
        Task<bool> WriteData(byte[] data);
    }

    public class NetworkClient : INetworkClient
    {
        /// <summary>
        /// Is the TCP connection connected?
        /// </summary>
        public bool Connected => _client?.Connected ?? false;
        /// <summary>
        /// The other side's IP Address
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// The other side's port
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// The other side's IP Addres
        /// </summary>
        public string IPAddress => $"{Host}:{Port}";

        /// <summary>
        /// The client;s network stream
        /// </summary>
        public NetworkStream Stream => _client?.GetStream();

        //networking client
        private TcpClient _client;

        /// <summary>
        /// When an exception occurs within the inner workings 
        /// (So we don't get any interuptions and can just log)
        /// </summary>
        public event ExceptionCarrier OnException = delegate { };
        /// <summary>
        /// When a client disconnects from the system
        /// </summary>
        public event NetworkClientCarrier OnDisconnected = delegate { };
        /// <summary>
        /// When data is received by the client
        /// </summary>
        public event NetworkDataCarrier OnDataReceived = delegate { };

        /// <summary>
        /// Default constructor for creating the session argument
        /// </summary>
        private NetworkClient()
        {
            
        }

        /// <summary>
        /// Connect to a listener
        /// </summary>
        /// <param name="host">The listener's DNS / IP</param>
        /// <param name="port">The listener's port</param>
        public NetworkClient(string host, int port) : this()
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Import connection from a listener
        /// </summary>
        /// <param name="client">The <see cref="TcpClient"/> obtained from the listener</param>
        public NetworkClient(TcpClient client) : this()
        {
            _client = client;
            var ip = ((IPEndPoint)client.Client.RemoteEndPoint);
            Host = ip.Address.ToString();
            Port = ip.Port;
        }

        /// <summary>
        /// Initiates the connection to the listener.
        /// </summary>
        /// <returns>Whether the connection was a success</returns>
        public async Task<bool> Start()
        {
            //      ,_      _,
            //        '.__.'
            //   '-,   (__)   ,-'
            //     '._ .::. _.'
            //       _'(^^)'_
            //    _,` `>\/<` `,_
            //   `  ,-` )( `-,  `
            //      |  /==\  |
            //    ,-'  |=-|  '-,
            //         )-=(
            //         \__/
            // HELP! THERES A BUG IN MY CODE!
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(Host, Port);

                ReadPacket();

                return Connected;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception ex)
            {
                OnException(ex, "Error Starting Client");
                return false;
            }
        }

        /// <summary>
        /// Stop the connection
        /// </summary>
        public void Stop()
        {
            try
            {
                _client.GetStream().Dispose();
                _client.Dispose();
            }
            catch { }

            OnDisconnected(this);
        }

        /// <summary>
        /// Write data to the connection's stream
        /// </summary>
        /// <param name="packet">The data to write</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<bool> WriteData(byte[] packet)
        {
            try
            {
                if (!Connected)
                {
                    Stop();
                    return false;
                }

                await _client.GetStream().WriteAsync(packet, 0, packet.Length);

                return true;
            }
            catch (Exception ex)
            {
                OnException(ex, "Error writing data");
                return false;
            }
        }

        private async void ReadPacket()
        {
            try
            {
                var buffer = new byte[_client.ReceiveBufferSize];
                int length;
                if ((length = await Stream.ReadAsync(buffer, 0, buffer.Length)) == 0)
                {
                    Stop();
                    return;
                }

                var data = new byte[length];
                Array.Copy(buffer, data, length);

                OnDataReceived(this, data);
            }
            catch (IOException)
            {
                Stop();
                return;
            }
            catch (Exception ex)
            {
                OnException(ex, "Error processing packet");
            }

            ReadPacket();
        }
    }
}
