namespace CloudStorage.Core.Entities;

public class StoredFile
{
    public Guid Id { get; set; }
    public required string OriginalName { get; set; }
    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public required string StoragePath { get; set; }
    public required string Sha256Hash { get; set; }
    public required DateTimeOffset UploadedAt { get; set; }

}