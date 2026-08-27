using CloudStorage.Storage;
namespace CloudStorage.IntergreationTests;

public class LocalFileTest
{
    [Fact]
    public async Task StoreAsync_WhenCalledWithValidData_CreatesFile()
    {
        var storage = new LocalFileStorage();

        var testData = "Hello, World!";
        var testPath = "testfile.txt";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(testData);
        var stream = new MemoryStream(bytes);

        string storedPath = await storage.StoreAsync(stream, testPath);

        Assert.True(File.Exists(storedPath));

        await storage.DeleteAsync(testPath);
    }

    [Fact]
    public async Task OpenReadAsync_WhenCalledWithExistingFile_ReturnsStream()
    {
        var storage = new LocalFileStorage();
        var testData = "OpenReadAsync Works!";
        var testPath = "OpenReadTest.txt";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(testData);

        var stream = new MemoryStream(bytes);

        await storage.StoreAsync(stream, testPath);

        using (var readStream = await storage.OpenReadAsync(testPath))
        {
            using (var reader = new StreamReader(readStream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.Equal(testData, content);
            }
        }

        await storage.DeleteAsync(testPath);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalledWithExistingFile_DeletesFile()
    {
        var storage = new LocalFileStorage();
        var testData = "DeleteAsync Works!";
        var testPath = "DeleteTest.txt";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(testData);
        var stream = new MemoryStream(bytes);

        await storage.StoreAsync(stream, testPath);

        bool deleted = await storage.DeleteAsync(testPath);

        Assert.True(deleted);
        Assert.False(File.Exists(Path.Combine("../../../../../Documents/Storage/", testPath)));
    }

    [Fact]
    public async Task DeleteAsync_WhenCalledWithNonExistingFile_ReturnsFalse()
    {
        var storage = new LocalFileStorage();
        var testPath = "NonExistingFile.txt";

        bool deleted = await storage.DeleteAsync(testPath);

        Assert.False(deleted);
    }

    [Fact]

    public async Task OpenReadAsync_WhenCalledWithNonExistentPath_ReturnsExcpetion()
    {
        var storage = new LocalFileStorage();
        var testPath = "doesnotexist.txt";

        var exception = await Assert.ThrowsAsync<Exception>(async () => await storage.OpenReadAsync(testPath));
    }

    [Fact]
    public async Task TaskStoreAsync_WhenCalledWithExistingFile_ReturnsException()
    {
        var storage = new LocalFileStorage();
        var testData = "StoreAsync Works!";
        var testPath = "StoreTest.txt";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(testData);
        var stream = new MemoryStream(bytes);

        await storage.StoreAsync(stream, testPath);

        var exception = await Assert.ThrowsAsync<Exception>(async () => await storage.StoreAsync(stream, testPath));

        await storage.DeleteAsync(testPath);
    }
}
