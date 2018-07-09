using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PalApi.Plugins
{
    using Delegates;
    using Types;
    using Utilities;
    using Networking.Mapping;

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

        private IReflectionUtility reflection;
        private IRoleManager roleManager;

        public PluginManager(IReflectionUtility reflection, IRoleManager roleManager)
        {
            this.reflection = reflection;
            this.roleManager = roleManager;
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

                if (plugin.InstanceCommand != null)
                {
                    if (!plugin.InstanceCommand.MessageType.HasFlag(message.MesgType))
                        continue;

                    if (!msg.ToLower().StartsWith(plugin.InstanceCommand.Cmd.ToLower()))
                        continue;

                    if (!string.IsNullOrEmpty(plugin.InstanceCommand.Roles) && !await roleManager.IsInRole(plugin.InstanceCommand.Roles, bot, message))
                        continue;

                    msg = msg.Remove(0, plugin.InstanceCommand.Cmd.Length).Trim();

                    if (string.IsNullOrEmpty(msg))
                    {
                        var defs = defaults.Where(t => t.Instance == plugin.Instance).ToArray();

                        if (defs.Length <= 0)
                            continue;

                        foreach(var d in defs)
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
                                return;
                            }
                            catch (Exception ex)
                            {
                                OnException(ex, $"Error running default plugin {plugin.InstanceCommand?.Cmd} {d.Method.Name} with \"{message.Content}\" from {message.UserId}");
                            }
                        }
                    }
                }

                if (!plugin.MethodCommand.MessageType.HasFlag(message.MesgType))
                    continue;

                if (!msg.ToLower().StartsWith(plugin.MethodCommand.Cmd.ToLower()))
                    continue;

                if (!string.IsNullOrEmpty(plugin.MethodCommand.Roles) && !await roleManager.IsInRole(plugin.MethodCommand.Roles, bot, message))
                    continue;

                msg = msg.Remove(0, plugin.MethodCommand.Cmd.Length).Trim();
                
                try
                {
                    plugin.Method.Invoke(plugin.Instance, new object[] { bot, message, msg });
                }
                catch (Exception ex)
                {
                    OnException(ex, $"Error running plugin {plugin.InstanceCommand?.Cmd} {plugin.MethodCommand.Cmd} with \"{message.Content}\" from {message.UserId}");
                }
            }
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
            var ic = plug.GetType().GetCustomAttributes<Command>().ToArray();

            var ms = plug.GetType()
                         .GetMethods()
                         .Where(t => Attribute.IsDefined(t, typeof(Command)))
                         .ToDictionary(t => t, t => t.GetCustomAttributes<Command>());

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
            public Command InstanceCommand { get; set; }
            public MethodInfo Method { get; set; }
            public Command MethodCommand { get; set; }
        }

        private class ReflectedDefault
        {
            public IPlugin Instance { get; set; }
            public Command InstanceCommand { get; set; }
            public MethodInfo Method { get; set; }
            public Default MethodDefault { get; set; }
        }
    }
}