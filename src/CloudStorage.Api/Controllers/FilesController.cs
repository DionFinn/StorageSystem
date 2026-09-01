using Microsoft.AspNetCore.Mvc;
using CloudStorage.Core.Entities;
using CloudStorage.Core.Interfaces;
using CloudStorage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CloudStorage.Api.Controllers;

[ApiController]
[Route("api/files")]


public class FilesController(CloudStorageDbContext context, IFileStorage fileStorage): ControllerBase
{
    private readonly CloudStorageDbContext _context = context;
    private readonly IFileStorage _fileStorage = fileStorage;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoredFile>>> GetStoredFiles()
    {
        return Ok(await _context.StoredFiles.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoredFile>> GetStoredFile(Guid id)
    {
        var file = await _context.StoredFiles.FindAsync(id);

        if(file == null)
        {
            return NotFound();
        }

        return file;
    }
// [TODO] will need to have cascading delete for the stored file in S3 as well as database entry.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStoredFile(Guid id)
    {
        var file = await _context.StoredFiles.FindAsync(id);

        if (file == null)
        {
            return NotFound();
        }

        await _fileStorage.DeleteAsync(file.StoragePath);
        _context.StoredFiles.Remove(file);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<StoredFile>> UpdateStoredFileName(
        Guid id,
        [FromBody] UpdateStoredFileNameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var file = await _context.StoredFiles.FindAsync(id);

        if (file == null)
        {
            return NotFound();
        }

        file.OriginalName = request.Name.Trim();
        await _context.SaveChangesAsync();

        return Ok(file);
    }

    [HttpPost]
    public async Task<ActionResult<StoredFile>> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        var hashbytes = await SHA256.HashDataAsync(stream);
        var hash = Convert.ToHexString(hashbytes);


        var storedFile = new StoredFile
        {
            Id = Guid.NewGuid(),
            OriginalName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            StoragePath = "",
            Sha256Hash = hash,
            UploadedAt = DateTimeOffset.UtcNow
        };

        storedFile.StoragePath = Path.Combine(storedFile.Id.ToString());

        using var storageStream = file.OpenReadStream();

        await _fileStorage.StoreAsync(storageStream, storedFile.StoragePath);
        _context.StoredFiles.Add(storedFile);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStoredFile), new { id = storedFile.Id }, storedFile);
    }
}

public sealed record UpdateStoredFileNameRequest(string Name);
