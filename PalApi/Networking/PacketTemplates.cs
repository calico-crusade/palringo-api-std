using System.Collections.Generic;

namespace PalApi.Networking
{
    using SubProfile;
    using SubProfile.Parsing;
    using Types;

    public interface IPacketTemplates
    {
        IPacket Login(string email, DeviceType device, bool spamFilter = false, string clientVersion = null, bool redirect = false);
        IPacket Auth(byte[] password, AuthStatus status);
        IPacket Ping();
        IPacket UserInfo(int id);
        IPacket Message(MessageType target, DataType type, int id, byte[] data);
        IPacket Message(MessageType target, DataType type, int id, string data);
        IPacket AdminAction(AdminActions action, int user, int group);
        IPacket AddContact(int user, string message = "I'd like to add you!");
        IPacket AddContactResponse(bool accept, int user);
        IPacket CreateGroup(string name, string description, string password = null);
        IPacket JoinGroup(string name, string password = null);
        IPacket LeaveGroup(int groupId);
        IPacket Bye();
        IPacket UpdateProfile(string name, string status);
        IPacket UpdateProfile(ExtendedUser user);
    }

    public class PacketTemplates : IPacketTemplates
    {
        public IPacket Login(string email, DeviceType device, bool spamFilter = false, string clientVersion = null, bool redirect = false)
        {
            int mflags = 786437;
            //mflags |= 0x4;
            //mflags |= 0x80000;
            if (spamFilter)
                mflags |= 0x1000;

            return new Packet
            {
                Command = "LOGON",
                Headers = new Dictionary<string, string>
                {
                    ["APP-TYPE"] = device.GetStrDevice(),
                    ["CAPABILITIES"] = mflags.ToString(),
                    ["CLIENT-VERSION"] = clientVersion ?? "2.8.1, 60842",
                    ["FW"] = "Win 6.2",
                    ["PROTOCOL-VERSION"] = "2.0",
                    ["NAME"] = email,
                    ["REDIRECT-COUNT"] = redirect ? "1" : "0"
                }
            };
        }

        public IPacket Auth(byte[] password, AuthStatus status)
        {
            return new Packet
            {
                Command = "AUTH",
                Headers = new Dictionary<string, string>
                {
                    ["ENCRYPTION-TYPE"] = "1",
                    ["ONLINE-STATUS"] = ((int)status).ToString()
                },
                Payload = password
            };
        }

        public IPacket Ping()
        {
            return new Packet { Command = "P" };
        }

        public IPacket UserInfo(int id)
        {
            var map = new DataMap();
            map.SetValue("Sub-Id", id);

            return new Packet
            {
                Command = "SUB PROFILE QUERY",
                Payload = map.Serialize()
            };
        }

        public IPacket Message(MessageType target, DataType type, int id, byte[] data)
        {
            return new Packet
            {
                Command = "MESG",
                Headers = new Dictionary<string, string>
                {
                    ["TARGET-ID"] = id.ToString(),
                    ["MESG-TARGET"] = ((int)target).ToString(),
                    ["CONTENT-TYPE"] = type.FromDataType()
                },
                Payload = data
            };
        }

        public IPacket Message(MessageType target, DataType type, int id, string data)
        {
            return Message(target, type, id, PacketSerializer.Outbound.GetBytes(data));
        }

        public IPacket AdminAction(AdminActions action, int user, int group)
        {
            return new Packet
            {
                Command = "GROUP ADMIN",
                Headers = new Dictionary<string, string>
                {
                    ["GROUP-ID"] = group.ToString(),
                    ["TARGET-ID"] = user.ToString(),
                    ["ACTION"] = ((int)action).ToString()
                }
            };
        }

        public IPacket AddContact(int user, string message = "I'd like to add you!")
        {
            return new Packet
            {
                Command = "CONTACT ADD",
                Headers = new Dictionary<string, string>
                {
                    ["TARGET-ID"] = user.ToString()
                },
                Content = message
            };
        }

        public IPacket AddContactResponse(bool accept, int user)
        {
            return new Packet
            {
                Command = "CONTACT ADD RESP",
                Headers = new Dictionary<string, string>
                {
                    ["ACCEPTED"] = accept ? "1" : "0",
                    ["SOURCE-ID"] = user.ToString()
                }
            };
        }

        public IPacket CreateGroup(string name, string description, string password = null)
        {
            return new Packet
            {
                Command = "GROUP CREATE",
                Headers = new Dictionary<string, string>
                {
                    ["NAME"] = name,
                    ["DESC"] = description
                },
                Content = string.IsNullOrEmpty(password) ? "" : password
            };
        }

        public IPacket JoinGroup(string name, string password = null)
        {
            return new Packet
            {
                Command = "GROUP SUBSCRIBE",
                Headers = new Dictionary<string, string>
                {
                    ["NAME"] = name
                },
                Content = string.IsNullOrEmpty(password) ? "" : password
            };
        }

        public IPacket LeaveGroup(int groupId)
        {
            return new Packet
            {
                Command = "GROUP UNSUB",
                Headers = new Dictionary<string, string>
                {
                    ["GROUP-ID"] = groupId.ToString()
                }
            };
        }

        public IPacket Bye()
        {
            return new Packet
            {
                Command = "BYE"
            };
        }

        public IPacket UpdateProfile(string name, string status)
        {
            return new Packet
            {
                Command = "CONTACT DETAIL",
                Headers = new Dictionary<string, string>
                {
                    ["NICKNAME"] = name,
                    ["STATUS"] = status
                }
            };
        }

        public IPacket UpdateProfile(ExtendedUser user)
        {
            var map = user.ToMap();
            var extMap = new DataMap();
            extMap.SetValueRaw("ext", map.Serialize());
            var dataMap = new DataMap();
            dataMap.SetValueRaw("user_data", extMap.Serialize());
            return new Packet
            {
                Command = "SUB PROFILE UPDATE",
                Payload = dataMap.Serialize()
            };
        }
    }
}
