using System.Collections.Generic;

namespace CloudStorage.Core.Models;

public class ChunkResult
{
    public required int Index { get; set; }
    public required List<byte> Data { get; set; }    
    public required long SizeBytes { get; set; }
    public required string Hash { get; set; }

}