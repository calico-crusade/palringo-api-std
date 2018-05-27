namespace PalApi.Networking.Watcher
{
    using Mapping;

    public interface IWatch
    {
        bool Validate(IPacketMap packet);
    }
}
