using System.Security.Cryptography;
using CloudStorage.Storage;

namespace CloudStorage.IntegrationTests;

public class FixedSizeChunkerTests
{
    private const int ChunkSize = 4 * 1024 * 1024;

    [Fact]
    public async Task ChunkFileAsync_EmptyStream_ReturnsNoChunks()
    {
        var chunks = await ReadAllChunksAsync(Stream.Null);

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task ChunkFileAsync_SmallerThanChunkSize_ReturnsOnePartialChunk()
    {
        var data = CreateData(ChunkSize - 1);

        var chunks = await ReadAllChunksAsync(new MemoryStream(data));

        var chunk = Assert.Single(chunks);
        AssertChunk(chunk, 0, data);
    }

    [Fact]
    public async Task ChunkFileAsync_ExactChunkSize_ReturnsOneFullChunk()
    {
        var data = CreateData(ChunkSize);

        var chunks = await ReadAllChunksAsync(new MemoryStream(data));

        var chunk = Assert.Single(chunks);
        AssertChunk(chunk, 0, data);
    }

    [Fact]
    public async Task ChunkFileAsync_MultipleChunksWithPartialEnd_ReturnsOrderedChunks()
    {
        var data = CreateData(ChunkSize * 2 + 17);

        var chunks = await ReadAllChunksAsync(new MemoryStream(data));

        Assert.Equal(3, chunks.Count);
        AssertChunk(chunks[0], 0, data[..ChunkSize]);
        AssertChunk(chunks[1], 1, data[ChunkSize..(ChunkSize * 2)]);
        AssertChunk(chunks[2], 2, data[(ChunkSize * 2)..]);
    }

    [Fact]
    public async Task ChunkFileAsync_StreamReturnsShortReads_StillFillsChunks()
    {
        var data = CreateData(ChunkSize + 23);

        var chunks = await ReadAllChunksAsync(new ShortReadStream(data, maxReadSize: 257));

        Assert.Equal(2, chunks.Count);
        AssertChunk(chunks[0], 0, data[..ChunkSize]);
        AssertChunk(chunks[1], 1, data[ChunkSize..]);
    }

    [Fact]
    public async Task ChunkFileAsync_NullStream_ThrowsArgumentNullException()
    {
        var chunker = new FixedSizeChunker();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in chunker.ChunkFileAsync(null!)) { }
        });
    }

    private static byte[] CreateData(int length)
    {
        var data = new byte[length];
        RandomNumberGenerator.Fill(data);
        return data;
    }

    private static async Task<List<CloudStorage.Core.Models.ChunkResult>> ReadAllChunksAsync(Stream stream)
    {
        var chunks = new List<CloudStorage.Core.Models.ChunkResult>();
        await foreach (var chunk in new FixedSizeChunker().ChunkFileAsync(stream))
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static void AssertChunk(CloudStorage.Core.Models.ChunkResult chunk, int index, byte[] expectedData)
    {
        Assert.Equal(index, chunk.Index);
        Assert.Equal(expectedData.Length, chunk.SizeBytes);
        Assert.Equal(expectedData, chunk.Data);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(expectedData)), chunk.Hash);
    }

    private sealed class ShortReadStream(byte[] data, int maxReadSize) : MemoryStream(data)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(buffer.Length, maxReadSize)], cancellationToken);
    }
}
