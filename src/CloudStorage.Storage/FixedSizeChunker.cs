using System.Security.Cryptography;
using CloudStorage.Core.Interfaces;
using CloudStorage.Core.Models;

namespace CloudStorage.Storage;

public class FixedSizeChunker : IChunker
{
    private const int ChunkSize = 4 * 1024 * 1024;

    public async IAsyncEnumerable<ChunkResult> ChunkFileAsync(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        var index = 0;

        while (true)
        {
            var buffer = new byte[ChunkSize];
            var bytesFilled = 0;

            while (bytesFilled < ChunkSize)
            {
                var bytesRead = await fileStream.ReadAsync(
                    buffer.AsMemory(bytesFilled, ChunkSize - bytesFilled));

                if (bytesRead == 0)
                {
                    break;
                }

                bytesFilled += bytesRead;
            }

            if (bytesFilled == 0)
            {
                yield break;
            }

            var data = bytesFilled == ChunkSize
                ? buffer
                : buffer.AsSpan(0, bytesFilled).ToArray();

            yield return new ChunkResult
            {
                Index = index++,
                Data = data,
                SizeBytes = bytesFilled,
                Hash = Convert.ToHexString(SHA256.HashData(data))
            };
        }
    }
}
