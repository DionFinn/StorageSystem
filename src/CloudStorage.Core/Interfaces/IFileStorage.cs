using CloudStorage.Core.Entities;

namespace CloudStorage.Core.Interfaces;
public interface IFileStorage
{
    // Stream is used to store the file data, allows handling large files without loading them directly into memory.
    // giving the path to the file, returns a StoredFile object
    Task<string> StoreAsync(Stream data, string path);
    // given the path to the file, returns a stream to read the file data.
    Task<Stream> OpenReadAsync(string path);
    //given a path to file, delete the file from storage, retunr true if succussfully deleted.
    Task<bool> DeleteAsync(string path);
}