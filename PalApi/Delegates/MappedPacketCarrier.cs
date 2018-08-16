namespace PalApi.Delegates
{
    using Networking.Mapping;

    public delegate void MappedPacketCarrier<T>(IPalBot bot, T packet) where T: IPacketMap;
}
