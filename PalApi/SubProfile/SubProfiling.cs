using System;
using System.Collections.Generic;
using System.Linq;

namespace PalApi.SubProfile
{
    using Parsing;
    using Utilities;

    public interface ISubProfiling
    {
        Dictionary<int, Group> Groups { get; }

        Dictionary<int, User> Users { get; }

        Dictionary<int, User> Contacts { get; }

        ExtendedUser Profile { get; }

        Dictionary<Group, IEnumerable<GroupUser>> GroupUsers { get; }

        void Process(byte[] data, int? iv, int? rk);
    }

    public class SubProfiling : ISubProfiling
    {
        private BroadcastUtility broadcast;

        public Dictionary<int, Group> Groups { get; set; } = new Dictionary<int, Group>();

        public Dictionary<int, User> Users { get; set; } = new Dictionary<int, User>();

        public Dictionary<int, User> Contacts { get; set; } = new Dictionary<int, User>();

        public Dictionary<Group, IEnumerable<GroupUser>> GroupUsers => GetGroupUsers();

        public ExtendedUser Profile { get; } = new ExtendedUser();

        public SubProfiling(IBroadcastUtility broadcast)
        {
            this.broadcast = (BroadcastUtility)broadcast;
        }

        public void Process(byte[] data, int? iv, int? rk)
        {
            DataMap map;
            if (iv.HasValue && rk.HasValue)
            {
                map = new DataMap();
                map.Deserialize(data, iv.Value, data.Length - iv.Value - rk.Value);
            }
            else
            {
                map = new DataMap(data);
            }

            try
            {
                var contacts = map.GetValueMapAll("contacts");
                if (contacts != null)
                {
                    foreach(var item in contacts.Data)
                    {
                        if (!int.TryParse(item.Key, out int id))
                            continue;

                        var cm = new DataMap(item.Value);
                        if (Users.ContainsKey(id))
                            Users[id].Process(cm);
                        else
                        {
                            var u = new User(id);
                            u.Process(cm);
                            Users.Add(id, u);
                        }
                    }
                }

                var groups = map.GetValueMap("group_sub");
                if (groups != null)
                {
                    foreach (var gm in groups.Data)
                    {
                        if (!int.TryParse(gm.Key, out int id))
                            continue;
                        if (Groups.ContainsKey(id))
                            Groups[id].Process(new DataMap(gm.Value));
                        else
                        {
                            var gp = new Group { Id = id };
                            gp.Process(new DataMap(gm.Value));
                            Groups.Add(id, gp);
                        }
                    }
                }

                var ext = map.GetValueMap("ext");
                if (ext != null)
                    Profile.Process(ext);

                if (map != null)
                    Profile.Process(map);
            }
            catch (Exception ex)
            {
                broadcast.BroadcastException(ex, "Error parsing sub profile data");
            }
        }
        
        private Dictionary<Group, IEnumerable<GroupUser>> GetGroupUsers()
        {
            return Groups
                .ToDictionary(
                                t => t.Value,
                                t => t.Value.Members
                                            .Select(a => 
                                                GroupUser.FromUser(
                                                    Users.ContainsKey(a.Key) ? 
                                                        Users[a.Key] : 
                                                        new User(a.Key), 
                                                    a.Value)
                                            )
                             );
        }
    }
}
