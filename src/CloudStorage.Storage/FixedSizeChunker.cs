using CloudStorage.Core.Interfaces;
using CloudStorage.Core.Models;

namespace CloudStorage.Storage
{
    public class FixedSizeChunker() : IChunker
    {
        private const int _chunkSize = 4 * 1024 * 1024;

        // public async IAsyncEnumerable<ChunkResult> ChunkFileAsync(Stream fileStream)
        // {
        //     var buffer = new byte[_chunkSize];

        //     int byteFilled = 0;
        //     int index = 0;

        //     while(byteFilled < _chunkSize)
        //     {
        //         int bytesRead = await fileStream.ReadAsync(
        //             buffer.AsMemory(byteFilled, _chunkSize - byteFilled));
                
        //         if(bytesRead == 0)
        //         {
                    
        //         }
        //         else
        //         {
                    
        //         }
        //     }
        // }
    }
}