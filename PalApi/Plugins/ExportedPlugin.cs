using System.Collections.Generic;

namespace PalApi.Plugins
{
    using Comparitors;
    using Types;

    public class ExportedPlugin : ICommand
    {
        public string Comparitor { get; set; }
        public MessageType MessageType { get; set; }
        public string Roles { get; set; }
        public string Grouping { get; set; }
        public string Description { get; set; }

        public List<ICommand> Commands { get; private set; }
        public bool HasDefault { get; set; }
        public bool IsStandAlone => Commands.Count == 0;

        public ExportedPlugin(List<ICommand> commands)
        {
            Commands = commands;
        }
    }
}
