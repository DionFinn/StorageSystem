using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using CloudStorage.Core.Interfaces;

namespace CloudStorage.Storage
{
	public class LocalFileStorage : IFileStorage
    {
        private readonly string _fileStoragePath = "./storage";
        public async Task<string> StoreAsync(Stream data, string path)
        {   
            string fullpath = Path.Combine(_fileStoragePath, path);

            if(!Directory.Exists(_fileStoragePath))
            {
                Directory.CreateDirectory(_fileStoragePath);
            }
            if(File.Exists(fullpath))
            {
                throw new Exception("file already exists: " + fullpath);
            }
            

            using (FileStream fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write)) 
            {
                await data.CopyToAsync(fs);
            }

            return fullpath;
        }

        public Task<Stream> OpenReadAsync(string path)
        {
            string fullpath = Path.Combine(_fileStoragePath, path);
            if(!Directory.Exists(_fileStoragePath))
            {
                throw new Exception("base path does not exist: " + fullpath);
            }

            if(!File.Exists(fullpath))
            {
                throw new Exception("file does not exists with provided path: " + fullpath);
            }

            FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return Task.FromResult<Stream>(fs);
        }

        public Task<bool> DeleteAsync(string path)
        {
            string fullpath = Path.Combine(_fileStoragePath, path);

            if(!Directory.Exists(_fileStoragePath))
            {
                throw new Exception("base path does not exist: " + fullpath);
            }

            if(!File.Exists(fullpath))
            {
                return Task.FromResult(false);
                
            }

            File.Delete(fullpath);

            return Task.FromResult(true);
        }
    }
}