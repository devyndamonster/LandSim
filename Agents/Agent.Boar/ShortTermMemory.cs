using System.Text.Json;
using System.IO.Compression;

namespace Agent.Boar
{
    public record ShortTermMemory
    {
        public Destination? WanderDestination { get; init; }

        public int WanderStepsRemaining { get; init; }

        public string ToCompressedString()
        {
            byte[] dataToCompress = JsonSerializer.SerializeToUtf8Bytes(this);
            byte[] compressedData = Compress(dataToCompress);
            return Convert.ToBase64String(compressedData);
        }

        public static ShortTermMemory? FromCompressedString(string compressedString)
        {
            try
            {
                byte[] compressed = Convert.FromBase64String(compressedString);
                byte[] decompressed = Decompress(compressed);
                return JsonSerializer.Deserialize<ShortTermMemory>(decompressed);
            } 
            catch(Exception)
            {
                return null;
            }
        }

        private static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();
                }
                return result.ToArray();
            }
        }

        private static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source, CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }
    }
}
