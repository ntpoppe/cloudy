using Cloudy.API.Requests.Files;
using Cloudy.Application.DTOs;
using Cloudy.Application.DTOs.Files;
using Cloudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController(IFileService fileService) : BaseController
{
    // POST endpoints
    [HttpPost("intent")]
    public async Task<ActionResult<CreateIntentResponse>> CreateIntent([FromBody] CreateIntentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FileName))
            return BadRequest("FileName required.");

        var contentType = string.IsNullOrWhiteSpace(req.ContentType)
            ? "application/octet-stream"
            : req.ContentType;

        var serviceRequest = new CreateUploadIntentRequest(
            req.FileName,
            contentType,
            req.SizeBytes,
            GetCurrentUserId(),
            TimeSpan.FromMinutes(10)
        );

        var result = await fileService.CreateUploadIntentAsync(serviceRequest, ct);

        return Ok(new CreateIntentResponse(result.Url, result.ObjectKey));
    }

    [HttpPost("{fileId}/finalize")]
    public async Task<ActionResult<FileDto>> Finalize([FromRoute] string fileId, [FromBody] FinalizeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ObjectKey) || string.IsNullOrWhiteSpace(req.OriginalName))
            return BadRequest("ObjectKey and OriginalName required.");

        var serviceRequest = new CreateMetadataRequest(
            req.ObjectKey,
            req.OriginalName,
            req.ContentType,
            req.SizeBytes,
            GetCurrentUserId()
        );

        var dto = await fileService.CreateMetadataAsync(serviceRequest, ct);
        return Ok(dto);
    }

    // GET endpoints
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

    [HttpGet("{id:int}/download-url")]
    public async Task<ActionResult<string>> GetDownloadUrl([FromRoute] int id, CancellationToken ct)
    {
        var serviceRequest = new GetDownloadUrlRequest(id, TimeSpan.FromMinutes(10));
        var url = await fileService.GetDownloadUrlAsync(serviceRequest, ct);
        return Ok(url);
    }

    [HttpGet("storage-usage")]
    public async Task<ActionResult<StorageUsageDto>> GetStorageUsage(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var usage = await fileService.GetStorageUsageAsync(userId, ct);
        return Ok(usage);
    }

    // PUT endpoints
    [HttpPut("{id:int}/mark-pending-deletion")]
    public async Task<IActionResult> MarkPendingDeletion([FromRoute] int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await fileService.MarkAsPendingDeletionAsync(id, userId, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/restore-pending-deletion")]
    public async Task<IActionResult> RestorePendingDeletion([FromRoute] int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await fileService.RestoreFromPendingDeletionAsync(id, userId, ct);
        return NoContent();
    }

    // PATCH endpoints
    [HttpPatch("{id:int}/rename")]
    public async Task<IActionResult> Rename([FromRoute] int id, [FromBody] RenameRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NewName))
            return BadRequest("NewName required.");

        var serviceRequest = new RenameFileRequest(id, GetCurrentUserId(), req.NewName);
        await fileService.RenameAsync(serviceRequest, ct);
        return NoContent();
    }

    // DELETE endpoints
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var serviceRequest = new DeleteFileRequest(id, GetCurrentUserId());
        await fileService.DeleteAsync(serviceRequest, ct);
        return NoContent();
    }
}
