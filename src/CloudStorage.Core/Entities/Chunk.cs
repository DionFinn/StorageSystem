namespace CloudStorage.Core.Entities;

public class Chunk
{
    public required string Hash { get; set; }
    public required long SizeBytes { get; set; }
    public required string StoragePath { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}

