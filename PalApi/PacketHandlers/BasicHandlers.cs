namespace PalApi.PacketHandlers
{
    using Networking;
    using Networking.Handling;
    using PacketTypes;
    using SubProfile;
    using SubProfile.Parsing;
    using Plugins;
    using Types;
    using System;

    public class BasicHandlers : IPacketHandler
    {
        private IPacketTemplates templates;
        private IPluginManager pluginManager;

        public BasicHandlers(IPacketTemplates templates, IPluginManager pluginManager)
        {
            this.templates = templates;
            this.pluginManager = pluginManager;
        }

        public async void PingHandler(PingRequest ping, IPalBot bot)
        {
            var sent = await bot.Write(templates.Ping());
        }

        public void SubProfileHandler(SubProfilePacket sub, IPalBot bot)
        {
            ((PalBot)bot).SubProfiling.Process(sub.Data, sub.IV, sub.RK);
        }

        public void BalanceQueryResultHandle(BalanceQueryResult res, IPalBot bot)
        {
            ((PalBot)bot).SubProfiling.Process(res.Payload, null, null);
        }
        
        public void SubProfileQueryResponse(SubProfileQueryResult result, IPalBot bot)
        {
            var map = result.Data;
            if (!map.ContainsKey("sub-id"))
                return;

            var id = map.GetValueInt("sub-id");

            if (!((PalBot)bot).SubProfiling.Users.ContainsKey(id))
                ((PalBot)bot).SubProfiling.Users.Add(id, new User());

            foreach(var m in map.EnumerateMaps())
            {
                ((PalBot)bot).SubProfiling.Users[id].Process(m);
            }
        }

        public void GroupUpdateHandler(GroupUpdate update, IPalBot bot)
        {
            var map = update.Payload;

            if (!((PalBot)bot).SubProfiling.Users.ContainsKey(update.UserId))
                ((PalBot)bot).SubProfiling.Users.Add(update.UserId, new User(update.UserId));

            if (!((PalBot)bot).SubProfiling.Groups.ContainsKey(update.GroupId))
                return;

            if (update.Type == GroupUpdateType.Leave &&
                ((PalBot)bot).SubProfiling.Groups[update.GroupId].Members.ContainsKey(update.UserId))
            {
                ((PalBot)bot).SubProfiling.Groups[update.GroupId].Members.Remove(update.UserId);
            }

            if (update.Type == GroupUpdateType.Join)
            {
                var nextMap = new DataMap(map);

                if (nextMap.ContainsKey("contacts"))
                    nextMap = nextMap.GetValueMap("contacts");
                if (nextMap.ContainsKey(update.UserId.ToString()))
                    nextMap = nextMap.GetValueMap(update.UserId.ToString());

                ((PalBot)bot).SubProfiling.Users[update.UserId].Process(nextMap);

                if (!((PalBot)bot).SubProfiling.Groups[update.GroupId].Members.ContainsKey(update.UserId))
                    ((PalBot)bot).SubProfiling.Groups[update.GroupId].Members.Add(update.UserId, Role.User);
                return;
            }

            
        }

        public void AdminActionHandler(AdminAction action, IPalBot bot)
        {
            Console.WriteLine("Action happened: " + action);
        }
    }
}
