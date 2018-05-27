namespace PalApi.Networking
{
    using ZLib;

    public interface IZLibCompression
    {
        byte[] Decompress(byte[] data);
        byte[] Compress(byte[] data);
    }

    public class ZLibCompression : IZLibCompression
    {
        public byte[] Compress(byte[] data)
        {
            return ZlibStream.CompressBuffer(data);
        }

        public byte[] Decompress(byte[] data)
        {
            return ZlibStream.UncompressBuffer(data);
        }
    }
}
