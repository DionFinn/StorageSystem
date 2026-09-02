namespace CloudStorage.Core.Entities;

public class FileChunk
{
    public required Guid FileId { get; set; }
    public required int ChunkIndex { get; set; }
    public required string ChunkHash { get; set; }
}