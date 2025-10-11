using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController(IFileService fileService) : ControllerBase
{

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

        var result = await fileService.CreateUploadIntentAsync(
            req.FileName,
            contentType,
            req.SizeBytes,
            GetCurrentUserId(),
            TimeSpan.FromMinutes(10),
            ct
        );

        var objectKey = result.ObjectKey;
        var url = result.Url;

        return Ok(new CreateIntentResponse(url, objectKey));
    }

    // Finalize metadata after client PUT to MinIO
    [HttpPost("{fileId}/finalize")]
    public async Task<ActionResult<FileDto>> Finalize([FromRoute] string fileId, [FromBody] FinalizeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ObjectKey) || string.IsNullOrWhiteSpace(req.OriginalName))
            return BadRequest("ObjectKey and OriginalName required.");

        var userId = GetCurrentUserId();
        var dto = await fileService.CreateMetadataAsync(req.ObjectKey, req.OriginalName, req.ContentType, req.SizeBytes, userId, ct);
        return Ok(dto);
    }

    [HttpGet("{id:int}/download-url")]
    public async Task<ActionResult<string>> GetDownloadUrl([FromRoute] int id, CancellationToken ct)
    {
        var url = await fileService.GetDownloadUrlAsync(id, TimeSpan.FromMinutes(10), ct);
        return Ok(url);
    }

    // Rename (metadata only)
    [HttpPatch("{id:int}/rename")]
    public async Task<IActionResult> Rename([FromRoute] int id, [FromBody] RenameRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NewName))
            return BadRequest("NewName required.");

        var userId = GetCurrentUserId();
        await fileService.RenameAsync(id, userId, req.NewName, ct);
        return NoContent();
    }

    // Delete (removes from MinIO + soft-deletes row)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await fileService.DeleteAsync(id, userId, ct);
        return NoContent();
    }
    
    // Mark a file as pending deletion
    [HttpPut("{id:int}/mark-pending-deletion")]
    public async Task<IActionResult> MarkPendingDeletion([FromRoute] int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await fileService.MarkAsPendingDeletionAsync(id, userId, ct);
        return NoContent();
    }
    
    // Restore a file from pending deletion
    [HttpPut("{id:int}/restore-pending-deletion")]
    public async Task<IActionResult> RestorePendingDeletion([FromRoute] int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await fileService.RestoreFromPendingDeletionAsync(id, userId, ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetAll(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var files = await fileService.GetAllAsync(userId, ct);
        return Ok(files);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FileDto>> GetById(int id, CancellationToken ct)
    {
        var file = await fileService.GetByIdAsync(id, ct);
        return Ok(file);
    }

    [HttpGet("storage-usage")]
    public async Task<ActionResult<StorageUsageDto>> GetStorageUsage(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var usage = await fileService.GetStorageUsageAsync(userId, ct);
        return Ok(usage);
    }
}
