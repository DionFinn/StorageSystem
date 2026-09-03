using CloudStorage.Core.Models;

namespace CloudStorage.Core.Interfaces;

public interface IChunker
{
    IAsyncEnumerable<ChunkResult> ChunkFileAsync(Stream fileStream);
}