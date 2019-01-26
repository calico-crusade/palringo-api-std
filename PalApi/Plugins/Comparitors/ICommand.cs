namespace PalApi.Plugins.Comparitors
{
    using Types;

    public interface ICommand
    {
        string Comparitor { get; }
        MessageType MessageType { get; set; }
        string Roles { get; set; }
        string Grouping { get; set; }
        string Description { get; set; }
    }
}
