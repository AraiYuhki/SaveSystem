using System.IO;
using System.IO.Compression;

namespace Xeon.SaveSystem
{
    public static class Compressor
    {
        public static byte[] Compress(byte[] rawData)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    gZipStream.Write(rawData, 0, rawData.Length);
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            using (var decompressedStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gZipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }
    }
}
