using CloudStorage.Api.Controllers;
using CloudStorage.Core.Entities;
using CloudStorage.Core.Interfaces;
using CloudStorage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.IntegrationTests;

public class FilesControllerTests
{
    [Fact]
    public async Task GetStoredFiles_ReturnsAllFiles()
    {
        await using var context = CreateContext();
        context.StoredFiles.AddRange(CreateFile("one.txt"), CreateFile("two.txt"));
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.GetStoredFiles();

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var files = Assert.IsAssignableFrom<IEnumerable<StoredFile>>(response.Value);
        Assert.Equal(2, files.Count());
    }

    [Fact]
    public async Task GetStoredFile_WhenFileExists_ReturnsFile()
    {
        await using var context = CreateContext();
        var file = CreateFile("report.pdf");
        context.StoredFiles.Add(file);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.GetStoredFile(file.Id);

        Assert.Same(file, result.Value);
    }

    [Fact]
    public async Task GetStoredFile_WhenFileDoesNotExist_ReturnsNotFound()
    {
        await using var context = CreateContext();

        var result = await CreateController(context).GetStoredFile(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStoredFileName_ChangesOnlyName()
    {
        await using var context = CreateContext();
        var file = CreateFile("old.txt");
        context.StoredFiles.Add(file);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.UpdateStoredFileName(
            file.Id, new UpdateStoredFileNameRequest("new.txt"));

        Assert.IsType<OkObjectResult>(result.Result);
        var saved = await context.StoredFiles.FindAsync(file.Id);
        Assert.Equal("new.txt", saved!.OriginalName);
        Assert.Equal("application/octet-stream", saved.ContentType);
        Assert.Equal("stored/old.txt", saved.StoragePath);
    }

    [Fact]
    public async Task DeleteStoredFile_RemovesDatabaseRecordAndStoredFile()
    {
        await using var context = CreateContext();
        var file = CreateFile("delete.txt");
        context.StoredFiles.Add(file);
        await context.SaveChangesAsync();
        var storage = new FakeFileStorage();
        var controller = new FilesController(context, storage);

        var result = await controller.DeleteStoredFile(file.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.StoredFiles.FindAsync(file.Id));
        Assert.Equal(file.StoragePath, storage.DeletedPath);
    }

    private static FilesController CreateController(CloudStorageDbContext context) =>
        new(context, new FakeFileStorage());

    private static CloudStorageDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CloudStorageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static StoredFile CreateFile(string name) => new()
    {
        Id = Guid.NewGuid(), OriginalName = name,
        ContentType = "application/octet-stream", SizeBytes = 10,
        StoragePath = $"stored/{name}", Sha256Hash = new string('a', 64),
        UploadedAt = DateTimeOffset.UtcNow
    };

    private sealed class FakeFileStorage : IFileStorage
    {
        public string? DeletedPath { get; private set; }
        public Task<string> StoreAsync(Stream data, string path) => Task.FromResult(path);
        public Task<Stream> OpenReadAsync(string path) => Task.FromResult<Stream>(Stream.Null);
        public Task<bool> DeleteAsync(string path)
        {
            DeletedPath = path;
            return Task.FromResult(true);
        }
    }
}
