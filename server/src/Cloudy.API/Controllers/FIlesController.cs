using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    // Helper method to get current user ID
    protected virtual int GetCurrentUserId()
    {
        if (User == null)
            throw new UnauthorizedAccessException("User not authenticated");

        string? userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out int userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return userId;
    }

    // Transport contracts
    public record CreateIntentRequest(string FileName, string ContentType, long SizeBytes);
    public record CreateIntentResponse(string UploadUrl, string FileId);
    public record FinalizeRequest(string ObjectKey, string OriginalName, string ContentType, long SizeBytes);
    public record RenameRequest(string NewName);

    // Create upload intent (presigned PUT) 
    [HttpPost("intent")]
    public async Task<ActionResult<CreateIntentResponse>> CreateIntent([FromBody] CreateIntentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FileName))
            return BadRequest("FileName required.");

        var contentType = string.IsNullOrWhiteSpace(req.ContentType)
            ? "application/octet-stream"
            : req.ContentType;

        var (objectKey, url, ttl) =
            await _fileService.CreateUploadIntentAsync(req.FileName, contentType, TimeSpan.FromMinutes(10), ct);

        return Ok(new CreateIntentResponse(url, objectKey));
    }

    // Finalize metadata after client PUT to MinIO
    [HttpPost("{fileId}/finalize")]
    public async Task<ActionResult<FileDto>> Finalize([FromRoute] string fileId, [FromBody] FinalizeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ObjectKey) || string.IsNullOrWhiteSpace(req.OriginalName))
            return BadRequest("ObjectKey and OriginalName required.");

        var userId = GetCurrentUserId();
        var dto = await _fileService.CreateMetadataAsync(req.ObjectKey, req.OriginalName, req.ContentType, req.SizeBytes, userId, ct);
        return Ok(dto);
    }

    [HttpGet("{id:int}/download-url")]
    public async Task<ActionResult<string>> GetDownloadUrl([FromRoute] int id, CancellationToken ct)
    {
        var url = await _fileService.GetDownloadUrlAsync(id, TimeSpan.FromMinutes(10), ct);
        return Ok(url);
    }

    // Rename (metadata only)
    [HttpPatch("{id:int}/rename")]
    public async Task<IActionResult> Rename([FromRoute] int id, [FromBody] RenameRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NewName))
            return BadRequest("NewName required.");

        await _fileService.RenameAsync(id, req.NewName, ct);
        return NoContent();
    }

    // Delete (removes from MinIO + soft-deletes row)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _fileService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetAll(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var files = await _fileService.GetAllAsync(userId, ct);
        return Ok(files);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FileDto>> GetById(int id, CancellationToken ct)
    {
        var file = await _fileService.GetByIdAsync(id, ct);
        return Ok(file);
    }
}
