using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PalApi
{
    using Networking;
    using Networking.Watcher;
    using PacketTypes;
    using Types;
    using SubProfile;

    public interface IPalBotSenders
    {
        Group GetGroup(int id);
        IEnumerable<GroupUser> GetGroupMembers(int id);
        Task<User> GetUser(int id);
        Task<User[]> GetUsers(params int[] ids);
        Task<bool> Group(int id, byte[] image);
        Task<Response> Group(int id, string message);
        Task<Message> NextGroupMessage(int groupId);
        Task<Message> NextGroupMessage(int groupId, int userId);
        Task<Message> NextMessage(Func<Message, bool> match);
        Task<Message> NextMessage(int userId);
        Task<Message> NextMessage(int userId, Func<Message, bool> match);
        Task<Message> NextMessage(Message msg);
        Task<bool> GetConfirmation(Message msg, string prompt);
        Task<bool> GetConfirmation(Message msg, string prompt, Func<Message, bool> acceptance);
        Task<bool> Private(int id, byte[] image);
        Task<Response> Private(int id, string message);
        Task<bool> Reply(Message msg, byte[] image);
        Task<Response> Reply(Message msg, string message);
        Task<bool> Login(string email, string password, AuthStatus status = AuthStatus.Online, DeviceType device = DeviceType.PC, bool spamFilter = false, bool enablePlugins = true);
        Task<Response> AdminAction(AdminActions action, int user, int group);
        Task<Response> AddContact(int user, string message = "I'd like to add you");
        Task<Response> AddContactResponse(bool accept, int user);
        Task<Response> CreateGroup(string name, string description, string password = null);
        Task<Response> JoinGroup(string name, string password = null);
        Task<Response> LeaveGroup(int group);
        Task<Response> UpdateProfile(string nickname, string status);
        Task<Response> UpdateProfile(ExtendedUser user);
        Task Disconnect();

        Dictionary<Group, IEnumerable<GroupUser>> Groups { get; }
        IEnumerable<User> Users { get; }
    }

    public partial class PalBot
    {
        private async Task<Response> SendAwaitResponse(IPacket packet)
        {
            var send = await Write(packet);

            if (!send)
            {
                return new Response
                {
                    Type = Type.Code,
                    What = What.MESG,
                    Code = Code.NOT_DELIVERED
                };
            }

            return await packetWatcher.Subscribe<Response>((t) => t.MessageId == packet.MessageId);
        }

        #region Message Sending
        private async Task<bool> SendMessage(MessageType target, DataType type, int id, byte[] data)
        {
            var packet = packetTemplates.Message(target, type, id, data);

            return await Write(packet);
        }
        private async Task<Response> SendMessage(MessageType target, DataType type, int id, string data)
        {
            var packet = packetTemplates.Message(target, type, id, data);

            var send = await Write(packet);

            if (!send)
            {
                return new Response
                {
                    Type = Type.Code,
                    What = What.MESG,
                    Code = Code.NOT_DELIVERED
                };
            }

            return await packetWatcher.Subscribe<Response>((t) => t.MessageId == packet.MessageId);
        }

        public async Task<Response> Reply(Message msg, string message)
        {
            return await SendMessage(msg.MesgType, DataType.Text, msg.ReturnAddress, message);
        }
        public async Task<bool> Reply(Message msg, byte[] image)
        {
            return await SendMessage(msg.MesgType, DataType.Image, msg.ReturnAddress, image);
        }
        
        public async Task<Response> Private(int id, string message)
        {
            return await SendMessage(MessageType.Private, DataType.Text, id, message);
        }
        public async Task<bool> Private(int id, byte[] image)
        {
            return await SendMessage(MessageType.Private, DataType.Image, id, image);
        }

        public async Task<Response> Group(int id, string message)
        {
            return await SendMessage(MessageType.Group, DataType.Text, id, message);
        }
        public async Task<bool> Group(int id, byte[] image)
        {
            return await SendMessage(MessageType.Group, DataType.Image, id, image);
        }

        public async Task<Message> NextMessage(int userId)
        {
            return await packetWatcher.Subscribe<Message>(t => t.MesgType == MessageType.Private && t.UserId == userId);
        }
        public async Task<Message> NextMessage(Message msg)
        {
            var type = msg.MesgType;
            var retAddr = msg.ReturnAddress;
            var id = msg.UserId;
            return await packetWatcher.Subscribe<Message>(t => t.MesgType == type && t.ReturnAddress == retAddr && t.UserId == id);
        }
        public async Task<Message> NextMessage(Func<Message, bool> match)
        {
            return await packetWatcher.Subscribe<Message>(t => match(t));
        }
        public async Task<Message> NextMessage(int userId, Func<Message, bool> match)
        {
            return await packetWatcher.Subscribe<Message>(t => match(t) && t.UserId == userId);
        }
        public async Task<Message> NextGroupMessage(int groupId)
        {
            return await packetWatcher.Subscribe<Message>(t => t.MesgType == MessageType.Group && t.ReturnAddress == groupId);
        }
        public async Task<Message> NextGroupMessage(int groupId, int userId)
        {
            return await packetWatcher.Subscribe<Message>(t =>
                t.MesgType == MessageType.Group &&
                t.ReturnAddress == groupId &&
                t.UserId == userId);
        }
        public async Task<bool> GetConfirmation(Message msg, string prompt, Func<Message, bool> acceptance)
        {
            await Reply(msg, prompt);
            return acceptance(await NextMessage(msg));
        }
        public async Task<bool> GetConfirmation(Message msg, string prompt)
        {
            return await GetConfirmation(msg, prompt, (m) => m.Content.ToLower().Trim().StartsWith("y"));
        }
        #endregion

        public async Task<User> GetUser(int id)
        {
            if (SubProfiling.Users.ContainsKey(id))
                return SubProfiling.Users[id];

            var pack = packetTemplates.UserInfo(id);

            if (!await Write(pack))
                return null;

            try
            {
                await packetWatcher.Subscribe<SubProfileQueryResult, Response>(t => t.Id == id, t => t.MessageId == pack.MessageId && t.Code != Code.OK);
            }
            catch (PacketException ex)
            {
                Console.WriteLine("Issue with thing: " + ex.Packet.Command + " - " + ex.Packet.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Issue with thing: " + ex.ToString());
            }

            if (SubProfiling.Users.ContainsKey(id))
                return SubProfiling.Users[id];

            return new User
            {
                Id = id,
                Nickname = ""
            };
        }
        public async Task<User[]> GetUsers(params int[] ids)
        {
            return await Task.WhenAll(ids.Select(t => GetUser(t)));
        }
        public Group GetGroup(int id)
        {
            if (!SubProfiling.Groups.ContainsKey(id))
                return null;

            return SubProfiling.Groups[id];
        }
        public IEnumerable<GroupUser> GetGroupMembers(int id)
        {
            var group = GetGroup(id);
            return group == null ? null : SubProfiling.GroupUsers[group];
        }
        public Dictionary<Group, IEnumerable<GroupUser>> Groups => SubProfiling.GroupUsers;
        public IEnumerable<User> Users => SubProfiling.Users.Select(t => t.Value);

        public async Task<bool> Login(string email, string password,
            AuthStatus status = AuthStatus.Online,
            DeviceType device = DeviceType.PC,
            bool spamFilter = false,
            bool enablePlugins = true)
        {
            this.Email = email;
            this.Password = password;
            this.Status = status;
            this.Device = device;
            this.SpamFilter = spamFilter;
            this.EnablePlugins = enablePlugins;

            var connected = await _client.Start();

            if (!connected)
            {
                _couldntConnect?.Invoke();
                return false;
            }

            if (!await Write(packetTemplates.Login(email, device, spamFilter)))
            {
                _couldntConnect?.Invoke();
                return false;
            }

            var resp = await packetWatcher.Subscribe(new LoginFailed(), new AuthRequest());

            if (resp.Packet is LoginFailed)
            {
                var reason = ((LoginFailed)resp.Packet).Reason;
                _loginFailed?.Invoke(reason);
                OnLoginFailed(reason);
                return false;
            }

            var auth = (AuthRequest)resp.Packet;

            var pwd = PacketSerializer.Outbound.GetBytes(password);
            pwd = authentication.GenerateAuth(auth.Key, pwd);

            if (!await Write(packetTemplates.Auth(pwd, status)))
            {
                _couldntConnect?.Invoke();
                return false;
            }

            var balanceQuery = await packetWatcher.Subscribe(new LoginFailed(), new BalanceQueryResult());

            if (balanceQuery.Packet is LoginFailed)
            {
                var reason = ((LoginFailed)balanceQuery.Packet).Reason;
                _loginFailed?.Invoke(reason);
                OnLoginFailed(reason);
                return false;
            }

            return true;
        }

        public async Task<Response> AdminAction(AdminActions action, int user, int group)
        {
            var packet = packetTemplates.AdminAction(action, user, group);

            return await SendAwaitResponse(packet);
        }

        public async Task<Response> AddContact(int user, string message = "I'd like to add you")
        {
            return await SendAwaitResponse(packetTemplates.AddContact(user, message));
        }

        public async Task<Response> AddContactResponse(bool accept, int user)
        {
            return await SendAwaitResponse(packetTemplates.AddContactResponse(accept, user));
        }

        public async Task<Response> CreateGroup(string name, string description, string password = null)
        {
            return await SendAwaitResponse(packetTemplates.CreateGroup(name, description, password));
        }

        public async Task<Response> JoinGroup(string name, string password = null)
        {
            return await SendAwaitResponse(packetTemplates.JoinGroup(name, password));
        }

        public async Task<Response> LeaveGroup(int group)
        {
            return await SendAwaitResponse(packetTemplates.LeaveGroup(group));
        }

        public async Task<Response> UpdateProfile(string nickname, string status)
        {
            return await SendAwaitResponse(packetTemplates.UpdateProfile(nickname, status));
        }

        public async Task<Response> UpdateProfile(ExtendedUser user)
        {
            return await SendAwaitResponse(packetTemplates.UpdateProfile(user));
        }

        public async Task Disconnect()
        {
            await Write(new Packet
            {
                Command = "BYE",
                Headers = new Dictionary<string, string>(),
                Payload = new byte[0]
            });
            _client.Stop();
        }

        public async Task<bool> UpdateAvatar(byte[] image)
        {
            var packet = packetTemplates.AvatarUpdate(image);
            return await Write(packet);
        }
    }
}
