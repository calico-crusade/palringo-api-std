using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace PalApi.Plugins
{
    using Comparitors;
    using Delegates;
    using Networking.Mapping;
    using Linguistics;
    using Types;
    using Roles;
    using Utilities;

    public interface IPluginManager
    {
        event ExceptionCarrier OnException;
        event MessageCarrier OnMessage;

        void Process(IPalBot bot, IPacketMap pkt);
    }

    public class PluginManager : IPluginManager
    {
        public event ExceptionCarrier OnException = delegate { };
        public event MessageCarrier OnMessage = delegate { };

        private List<ReflectedPlugin> plugins;
        private List<ReflectedPacket> packets;
        private List<ReflectedDefault> defaults;

        private List<IComparitorProfile> comparitors;

        private IReflectionUtility reflection;
        private IRoleManager roleManager;

        public PluginManager(IReflectionUtility reflection, IRoleManager roleManager)
        {
            this.reflection = reflection;
            this.roleManager = roleManager;
            comparitors = new List<IComparitorProfile> { new LanguageComparitor(), new CommandComparitor() };
        }

        private async void ProcessMessage(IPalBot bot, Message message)
        {
            if (message.ContentType != DataType.Text)
                return;

            if (plugins == null || defaults == null)
                LoadPlugins();
            
            foreach(var plugin in plugins)
            {
                string msg = message.Content.Trim();
                Message tmpM = message.Clone();

                if (message is LangMessage)
                    ((LangMessage)message).LanguageKey = null;

                if (plugin.InstanceCommand != null)
                {
                    if (!string.IsNullOrEmpty(plugin.InstanceCommand.Roles) && 
                        !await roleManager.IsInRole(plugin.InstanceCommand.Roles, bot, message))
                        continue;

                    var c2 = CheckCommand(bot, plugin.InstanceCommand, message, msg, out tmpM);
                    if (c2 == null)
                        continue;

                    msg = c2;

                    if (string.IsNullOrEmpty(msg))
                    {
                        var defs = await DoDefaults(bot, plugin.Instance, message, plugin.InstanceCommand);

                        if (defs)
                            continue;
                        else
                            return;
                    }
                }

                if (!string.IsNullOrEmpty(plugin.MethodCommand.Roles) &&
                    !await roleManager.IsInRole(plugin.MethodCommand.Roles, bot, message))
                    continue;

                var c = CheckCommand(bot, plugin.MethodCommand, tmpM, msg, out tmpM);
                if (c == null)
                    continue;

                msg = c;
                
                try
                {
                    plugin.Method.Invoke(plugin.Instance, new object[] { bot, tmpM, msg });
                }
                catch (Exception ex)
                {
                    OnException(ex, $"Error running plugin {plugin.InstanceCommand?.Comparitor} {plugin.MethodCommand.Comparitor} with \"{message.Content}\" from {message.UserId}");
                }
            }
        }

        private string CheckCommand(IPalBot bot, ICommand cmd, Message message, string msg, out Message msgToUser)
        {
            msgToUser = message;
            if (!cmd.MessageType.HasFlag(message.MesgType))
                return null;

            if (!string.IsNullOrEmpty(cmd.Grouping) && 
                bot.Groupings != null && 
                bot.Groupings.Length > 0 && 
                !bot.Groupings.Any(t => 
                    t.ToLower().Trim() == cmd.Grouping.ToLower().Trim()))
                return null;

            var comp = comparitors.FirstOrDefault(t => t.AttributeType == cmd.GetType());
            if (comp == null)
                return null;


            if (comp.IsMatch(bot, message, msg, cmd, out string capped, out msgToUser))
                return capped.Trim();

            return null;
        }

        private async Task<bool> DoDefaults(IPalBot bot, IPlugin plugin, Message message, ICommand cmd)
        {
            var defs = defaults.Where(t => t.Instance == plugin).ToArray();

            if (defs.Length <= 0)
                return true;

            foreach (var d in defs)
            {
                if (!d.InstanceCommand.MessageType.HasFlag(message.MesgType))
                    continue;

                if (!string.IsNullOrEmpty(d.InstanceCommand.Roles) &&
                    !await roleManager.IsInRole(d.InstanceCommand.Roles, bot, message))
                    continue;

                try
                {
                    d.Method.Invoke(d.Instance,
                        d.Method.GetParameters().Length == 3 ?
                            new object[] { bot, message, "" } : //Fill in "string cmd" blank.
                            new object[] { bot, message });
                    return false;
                }
                catch (Exception ex)
                {
                    OnException(ex, $"Error running default plugin {cmd?.Comparitor} {d.Method.Name} with \"{message.Content}\" from {message.UserId}");
                }
            }

            return true;
        }

        public void Process(IPalBot bot, IPacketMap pkt)
        {
            if (pkt is Message && bot.EnablePlugins)
            {
                var msg = (Message)pkt;
                ProcessMessage(bot, msg);
                OnMessage(bot, msg);
                return;
            }

            if (packets == null)
                LoadPacketReflectors();

            var reflected = packets.Where(t => t.Command == pkt.Command).ToArray();

            foreach(var reflect in reflected)
            {
                try
                {
                    reflect.Method.Invoke(reflect.Instance, new object[] { pkt, bot });
                }
                catch (Exception ex)
                {
                    OnException(ex, $"Error running packet watcher {reflect.Command}");
                }
            }
        }

        private void LoadPlugins()
        {
            plugins = new List<ReflectedPlugin>();
            defaults = new List<ReflectedDefault>();

            var plugs = reflection.GetAllTypesOf<IPlugin>().ToArray();

            foreach(var plug in plugs)
            {
                HandlePluginLoad(plug);
                HandleDefaultLoad(plug);
            }
        }

        private void HandlePluginLoad(IPlugin plug)
        {
            var ic = plug.GetType().GetCustomAttributes<BaseCommand>().ToArray();

            var ms = plug.GetType()
                         .GetMethods()
                         .Where(t => Attribute.IsDefined(t, typeof(BaseCommand)))
                         .ToDictionary(t => t, t => t.GetCustomAttributes<BaseCommand>());

            if (ms.Count <= 0)
                return;

            if (ic.Length <= 0)
            {
                plugins.AddRange(ms.SelectMany(t => t.Value.Select(a => new ReflectedPlugin
                {
                    Instance = plug,
                    InstanceCommand = null,
                    Method = t.Key,
                    MethodCommand = a
                })));
                return;
            }

            foreach (var i in ic)
            {
                plugins.AddRange(ms.SelectMany(t => t.Value.Select(a => new ReflectedPlugin
                {
                    Instance = plug,
                    InstanceCommand = i,
                    Method = t.Key,
                    MethodCommand = a
                })));
            }
        }

        private void HandleDefaultLoad(IPlugin plug)
        {
            var ic = plug.GetType().GetCustomAttributes<Command>().ToArray();

            var ms = plug.GetType()
                         .GetMethods()
                         .Where(t => Attribute.IsDefined(t, typeof(Default)))
                         .ToDictionary(t => t, t => t.GetCustomAttributes<Default>());

            if (ms.Count <= 0)
                return;

            if (ic.Length <= 0)
                return;

            foreach(var i in ic)
            {
                defaults.AddRange(ms.SelectMany(t => t.Value.Select(a => new ReflectedDefault
                {
                    Instance = plug,
                    InstanceCommand = i,
                    Method = t.Key,
                    MethodDefault = a
                })));
            }
        }
        
        private void LoadPacketReflectors()
        {
            packets = new List<ReflectedPacket>();

            var plugs = reflection
                .GetTypes(typeof(IPlugin))
                .Select(t => (IPlugin)reflection.GetInstance(t))
                .ToArray();

            foreach(var plug in plugs)
            {
                var meths = plug.GetType()
                                .GetMethods()
                                .Where(t => Attribute.IsDefined(t, typeof(PacketWatcher)))
                                .ToArray();

                foreach(var meth in meths)
                {
                    var firstParam = meth.GetParameters().FirstOrDefault();
                    if (firstParam == null)
                        continue;

                    if (!firstParam.ParameterType.GetTypeInfo().ImplementedInterfaces.Any(t => t == typeof(IPacketMap)))
                        continue;

                    if (firstParam.ParameterType.IsAbstract || firstParam.ParameterType.IsInterface)
                        continue;

                    var pack = (IPacketMap)Activator.CreateInstance(firstParam.ParameterType);

                    packets.Add(new ReflectedPacket
                    {
                        Instance = plug,
                        Method = meth,
                        Command = pack.Command
                    });
                }
            }
        }
        
        private class ReflectedPacket
        {
            public IPlugin Instance { get; set; }
            public MethodInfo Method { get; set; }
            public string Command { get; set; }
        }

        private class ReflectedPlugin
        {
            public IPlugin Instance { get; set; }
            public ICommand InstanceCommand { get; set; }
            public MethodInfo Method { get; set; }
            public ICommand MethodCommand { get; set; }
        }

        private class ReflectedDefault
        {
            public IPlugin Instance { get; set; }
            public ICommand InstanceCommand { get; set; }
            public MethodInfo Method { get; set; }
            public Default MethodDefault { get; set; }
        }
    }
}