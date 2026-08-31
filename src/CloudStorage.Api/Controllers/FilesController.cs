using Microsoft.AspNetCore.Mvc;
using CloudStorage.Core.Entities;
using CloudStorage.Infrastructure.Persistence;

namespace CloudStorage.Api.Controllers;

[ApiController]
[Route("api/files")]


public class FilesController(CloudStorageDbContext context): ControllerBase
{
    private readonly CloudStorageDbContext _context = context;

    [HttpGet("files/{id}")]
    public async Task<ActionResult<StoredFile>> GetStoredFile(Guid id)
    {
        var file = await _context.StoredFiles.FindAsync(id);

        if(file == null)
        {
            throw new Exception($"File not found with ID: {id}");
        } else
        {
            return file;
        }
    }
}