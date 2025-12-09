using Cloudy.API.Controllers;
using Cloudy.API.Requests.Files;
using Cloudy.Application.DTOs;
using Cloudy.Application.DTOs.Files;
using Cloudy.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Cloudy.API.Tests;

public class FilesControllerTests
{
    private readonly Mock<IFileService> _fileService = new();
    private readonly TestFilesController _controller;

    public FilesControllerTests()
    {
        _controller = new TestFilesController(_fileService.Object);
    }

    // Test controller that overrides GetCurrentUserId for testing
    private class TestFilesController : FilesController
    {
        public TestFilesController(IFileService fileService) : base(fileService) { }

        protected override int GetCurrentUserId() => 1; // Return mock user ID for tests
    }

    [Fact]
    public async Task CreateIntent_Should_Return_Ok_With_PresignedUrl()
    {
        // Arrange
        var request = new CreateIntentRequest("test.txt", "text/plain", 1024);
        var expectedObjectKey = "guid-test.txt";
        var expectedUrl = "https://presigned-put-url";
        var expectedTtl = 600;
        var expectedResponse = new CreateUploadIntentResponse(expectedObjectKey, expectedUrl, expectedTtl);

        _fileService
            .Setup(s => s.CreateUploadIntentAsync(It.IsAny<CreateUploadIntentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var actionResult = await _controller.CreateIntent(request, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = result.Value.Should().BeOfType<CreateIntentResponse>().Subject;
        
        response.UploadUrl.Should().Be(expectedUrl);
        response.FileId.Should().Be(expectedObjectKey);

        _fileService.Verify(s => s.CreateUploadIntentAsync(
            It.Is<CreateUploadIntentRequest>(r => r.FileName == request.FileName && r.SizeBytes == request.SizeBytes), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIntent_Should_Return_BadRequest_If_FileName_Empty()
    {
        // Arrange
        var request = new CreateIntentRequest("", "text/plain", 1024);

        // Act
        var actionResult = await _controller.CreateIntent(request, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        result.Value.Should().Be("FileName required.");
    }

    [Fact]
    public async Task CreateIntent_Should_Use_Default_ContentType_If_Empty()
    {
        // Arrange
        var request = new CreateIntentRequest("test.txt", "", 1024);
        var expectedObjectKey = "guid-test.txt";
        var expectedUrl = "https://presigned-put-url";
        var expectedTtl = 600;
        var expectedResponse = new CreateUploadIntentResponse(expectedObjectKey, expectedUrl, expectedTtl);

        _fileService
            .Setup(s => s.CreateUploadIntentAsync(It.IsAny<CreateUploadIntentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var actionResult = await _controller.CreateIntent(request, CancellationToken.None);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _fileService.Verify(s => s.CreateUploadIntentAsync(
            It.Is<CreateUploadIntentRequest>(r => r.ContentType == "application/octet-stream"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Finalize_Should_Return_Ok_With_FileDto()
    {
        // Arrange
        var fileId = "test-key";
        var request = new FinalizeRequest("test-key", "test.txt", "text/plain", 1024);
        var expectedDto = new FileDto(1, "test.txt", 1024, "text/plain", DateTime.UtcNow, "cloudy", "test-key", false);

        _fileService
            .Setup(s => s.CreateMetadataAsync(It.IsAny<CreateMetadataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var actionResult = await _controller.Finalize(fileId, request, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = result.Value.Should().BeOfType<FileDto>().Subject;
        
        response.Id.Should().Be(expectedDto.Id);
        response.Name.Should().Be(expectedDto.Name);

        _fileService.Verify(s => s.CreateMetadataAsync(
            It.Is<CreateMetadataRequest>(r => r.ObjectKey == request.ObjectKey && r.OriginalName == request.OriginalName), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Finalize_Should_Return_BadRequest_If_ObjectKey_Empty()
    {
        // Arrange
        var fileId = "test-key";
        var request = new FinalizeRequest("", "test.txt", "text/plain", 1024);

        // Act
        var actionResult = await _controller.Finalize(fileId, request, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        result.Value.Should().Be("ObjectKey and OriginalName required.");
    }

    [Fact]
    public async Task Finalize_Should_Return_BadRequest_If_OriginalName_Empty()
    {
        // Arrange
        var fileId = "test-key";
        var request = new FinalizeRequest("test-key", "", "text/plain", 1024);

        // Act
        var actionResult = await _controller.Finalize(fileId, request, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        result.Value.Should().Be("ObjectKey and OriginalName required.");
    }

    [Fact]
    public async Task GetDownloadUrl_Should_Return_Ok_With_PresignedUrl()
    {
        // Arrange
        var fileId = 1;
        var expectedUrl = "https://presigned-get-url";

        _fileService
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<GetDownloadUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var actionResult = await _controller.GetDownloadUrl(fileId, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = result.Value.Should().BeOfType<string>().Subject;
        
        response.Should().Be(expectedUrl);

        _fileService.Verify(s => s.GetDownloadUrlAsync(
            It.Is<GetDownloadUrlRequest>(r => r.FileId == fileId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rename_Should_Return_NoContent()
    {
        // Arrange
        var fileId = 1;
        var request = new RenameRequest("new-name.txt");

        _fileService
            .Setup(s => s.RenameAsync(It.IsAny<RenameFileRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await _controller.Rename(fileId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();

        _fileService.Verify(s => s.RenameAsync(
            It.Is<RenameFileRequest>(r => r.FileId == fileId && r.NewName == request.NewName), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rename_Should_Return_BadRequest_If_NewName_Empty()
    {
        // Arrange
        var fileId = 1;
        var request = new RenameRequest("");

        // Act
        var actionResult = await _controller.Rename(fileId, request, CancellationToken.None);

        // Assert
        var result = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        result.Value.Should().Be("NewName required.");
    }

    [Fact]
    public async Task Delete_Should_Return_NoContent()
    {
        // Arrange
        var fileId = 1;

        _fileService
            .Setup(s => s.DeleteAsync(It.IsAny<DeleteFileRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await _controller.Delete(fileId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();

        _fileService.Verify(s => s.DeleteAsync(
            It.Is<DeleteFileRequest>(r => r.FileId == fileId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Files()
    {
        // Arrange
        var expectedFiles = new List<FileDto>
        {
            new(1, "file1.txt", 1024, "text/plain", DateTime.UtcNow, "cloudy", "key1", false),
            new(2, "file2.pdf", 2048, "application/pdf", DateTime.UtcNow, "cloudy", "key2", false)
        };

        _fileService
            .Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFiles);

        // Act
        var actionResult = await _controller.GetAll(CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = result.Value.Should().BeAssignableTo<IEnumerable<FileDto>>().Subject;
        var filesList = response.ToList();
        
        filesList.Should().HaveCount(2);
        filesList[0].Id.Should().Be(expectedFiles[0].Id);
        filesList[1].Id.Should().Be(expectedFiles[1].Id);

        _fileService.Verify(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_Return_Ok_With_File()
    {
        // Arrange
        var fileId = 1;
        var expectedFile = new FileDto(fileId, "test.txt", 1024, "text/plain", DateTime.UtcNow, "cloudy", "test-key", false);

        _fileService
            .Setup(s => s.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFile);

        // Act
        var actionResult = await _controller.GetById(fileId, CancellationToken.None);

        // Assert
        var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = result.Value.Should().BeOfType<FileDto>().Subject;
        
        response.Id.Should().Be(expectedFile.Id);
        response.Name.Should().Be(expectedFile.Name);
        response.Size.Should().Be(expectedFile.Size);

        _fileService.Verify(s => s.GetByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIntent_Should_Propagate_CancellationToken()
    {
        // Arrange
        var request = new CreateIntentRequest("test.txt", "text/plain", 1024);
        using var cts = new CancellationTokenSource();
        var expected = new CreateUploadIntentResponse("guid-test.txt", "https://presigned-put-url", 600);

        _fileService
            .Setup(s => s.CreateUploadIntentAsync(It.IsAny<CreateUploadIntentRequest>(), cts.Token))
            .ReturnsAsync(expected);

        // Act
        var actionResult = await _controller.CreateIntent(request, cts.Token);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _fileService.Verify(s => s.CreateUploadIntentAsync(
            It.IsAny<CreateUploadIntentRequest>(),
            It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
    }

    [Fact]
    public async Task GetDownloadUrl_Should_Propagate_CancellationToken()
    {
        // Arrange
        var fileId = 1;
        var cts = new CancellationTokenSource();
        var expectedUrl = "https://presigned-get-url";

        _fileService
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<GetDownloadUrlRequest>(), cts.Token))
            .ReturnsAsync(expectedUrl);

        // Act
        var actionResult = await _controller.GetDownloadUrl(fileId, cts.Token);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _fileService.Verify(s => s.GetDownloadUrlAsync(
            It.Is<GetDownloadUrlRequest>(r => r.FileId == fileId), 
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Rename_Should_Propagate_CancellationToken()
    {
        // Arrange
        var fileId = 1;
        var request = new RenameRequest("new-name.txt");
        var cts = new CancellationTokenSource();

        _fileService
            .Setup(s => s.RenameAsync(It.IsAny<RenameFileRequest>(), cts.Token))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await _controller.Rename(fileId, request, cts.Token);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        _fileService.Verify(s => s.RenameAsync(
            It.Is<RenameFileRequest>(r => r.FileId == fileId && r.NewName == request.NewName), 
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Propagate_CancellationToken()
    {
        // Arrange
        var fileId = 1;
        var cts = new CancellationTokenSource();

        _fileService
            .Setup(s => s.DeleteAsync(It.IsAny<DeleteFileRequest>(), cts.Token))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await _controller.Delete(fileId, cts.Token);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        _fileService.Verify(s => s.DeleteAsync(
            It.Is<DeleteFileRequest>(r => r.FileId == fileId), 
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetAll_Should_Propagate_CancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var expectedFiles = new List<FileDto>
        {
            new(1, "file1.txt", 1024, "text/plain", DateTime.UtcNow, "cloudy", "key1", false)
        };

        _fileService
            .Setup(s => s.GetAllAsync(It.IsAny<int>(), cts.Token))
            .ReturnsAsync(expectedFiles);

        // Act
        var actionResult = await _controller.GetAll(cts.Token);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _fileService.Verify(s => s.GetAllAsync(It.IsAny<int>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_Propagate_CancellationToken()
    {
        // Arrange
        var fileId = 1;
        var cts = new CancellationTokenSource();
        var expectedFile = new FileDto(fileId, "test.txt", 1024, "text/plain", DateTime.UtcNow, "cloudy", "test-key", false);

        _fileService
            .Setup(s => s.GetByIdAsync(fileId, cts.Token))
            .ReturnsAsync(expectedFile);

        // Act
        var actionResult = await _controller.GetById(fileId, cts.Token);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _fileService.Verify(s => s.GetByIdAsync(fileId, cts.Token), Times.Once);
    }
}
