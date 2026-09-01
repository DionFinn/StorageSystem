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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStoredFile(Guid id)
    {
        var file = await _context.StoredFiles.FindAsync(id);

        if (file == null)
        {
            return NotFound();
        }

        try
        {
            _context.StoredFiles.Remove(file);
            await _context.SaveChangesAsync();

            await _fileStorage.DeleteAsync(file.StoragePath);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

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

        bool fileStored = false;

        try
        {
            await _fileStorage.StoreAsync(storageStream, storedFile.StoragePath);
            fileStored = true;

            _context.StoredFiles.Add(storedFile);
            await _context.SaveChangesAsync();
        
        } catch (Exception ex)
        {   
            if(fileStored == true)
            {
                await _fileStorage.DeleteAsync(storedFile.StoragePath);
            }
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }    
        return CreatedAtAction(nameof(GetStoredFile), new { id = storedFile.Id }, storedFile);
    }
}

public sealed record UpdateStoredFileNameRequest(string Name);
