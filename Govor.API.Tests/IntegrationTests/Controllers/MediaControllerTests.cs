using System.Text;
using AutoFixture;
using Govor.API.Controllers;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.Requests;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class MediaControllerTests
{
    private Fixture _fixture;
    private Mock<ICurrentUserService> _currentUserMock;
    private Mock<ILogger<MediaController>> _loggerMock;
    private Mock<IMediaService> _mockMedia;
    private Mock<IAccesserToDownloadMedia> _mockAccesser;
    private MediaController _controller;
    private Guid _userId = Guid.NewGuid();
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<MediaController>>();
        _mockAccesser = new Mock<IAccesserToDownloadMedia>();
        _mockMedia = new Mock<IMediaService>();
        
        _currentUserMock.Setup(f => f.GetCurrentUserId()).Returns(_userId);
        
        _controller = new MediaController(
            _loggerMock.Object,
            _mockMedia.Object,
            _mockAccesser.Object,
            _currentUserMock.Object);
    }
    
    // Tests for Upload action 
    [Test]
    public async Task Upload_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var content = "fake file content";
        var fileName = "testfile.txt";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(fileBytes);
    
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(fileBytes.Length);
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns<Stream, CancellationToken>((target, _) => stream.CopyToAsync(target));

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.Type, MediaType.Image)
            .With(r => r.MimeType, "image/png")
            .With(r => r.EncryptedKey, "secret")
            .Create();

        var uploadResult = _fixture.Create<MediaUploadResult>();

        _mockMedia.Setup(f => f.UploadMediaAsync(It.IsAny<Media>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        var value = okResult?.Value as MediaUploadResult;

        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Url, Is.EqualTo(uploadResult.Url));
        Assert.That(value.MediaId, Is.EqualTo(uploadResult.MediaId));
    }

    [Test]
    public async Task Upload_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");
        var content = "fake file content";
        var fileName = "testfile.txt";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(fileBytes);
    
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(fileBytes.Length);
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns<Stream, CancellationToken>((target, _) => stream.CopyToAsync(target));

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.Type, MediaType.Image)
            .With(r => r.MimeType, "image/png")
            .With(r => r.EncryptedKey, "secret")
            .Create();

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Upload_NoFileUploaded_ReturnsBadRequest()
    {
        // Arrange
        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, (IFormFile)null)
            .Create();

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("No file uploaded"));
    }

    [Test]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(0);

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .Create();

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("No file uploaded"));
    }

    [Test]
    public async Task Upload_FileTooLarge_ReturnsBadRequest()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(20_000_001); // Just over 20MB

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .Create();

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("File is too large"));
    }

    [Test]
    public async Task Upload_MissingMimeType_ReturnsBadRequest()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1000);
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.MimeType, string.Empty)
            .Create();

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("Missing MIME type"));
    }

    [Test]
    public async Task Upload_UnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1000);
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");
        formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1000]));

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.MimeType, "text/plain")
            .Create();

        _mockMedia.Setup(f => f.UploadMediaAsync(It.IsAny<Media>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());
        var forbidResult = result as ForbidResult;
        Assert.That(forbidResult.AuthenticationSchemes.First(), Is.EqualTo("Access denied"));
    }

    [Test]
    public async Task Upload_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1000);
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");
        formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1000]));

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.MimeType, "text/plain")
            .Create();

        _mockMedia.Setup(f => f.UploadMediaAsync(It.IsAny<Media>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid operation"));
    }

    [Test]
    public async Task Upload_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1000);
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");
        formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1000]));

        var uploadRequest = _fixture.Build<MediaUploadRequest>()
            .With(r => r.FromFile, formFileMock.Object)
            .With(r => r.MimeType, "text/plain")
            .Create();

        _mockMedia.Setup(f => f.UploadMediaAsync(It.IsAny<Media>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.Upload(uploadRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    // Tests for Download action
    [Test]
    public async Task Download_HasAccessAndMediaExists_ReturnsFile()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var media = _fixture.Build<Media>()
            .With(m => m.Data, Encoding.UTF8.GetBytes("fake file content"))
            .With(m => m.MimeType, "application/octet-stream") // Ensure MimeType is set
            .With(m => m.FileName, "testfile.txt")
            .Create();

        _mockAccesser.Setup(f => f.HasAccessAsync(mediaId, _userId)).ReturnsAsync(true);
        _mockMedia.Setup(f => f.GetMediaByIdAsync(mediaId)).ReturnsAsync(media);

        // Act
        var result = await _controller.Download(mediaId);

        // Assert
        Assert.That(result, Is.InstanceOf<FileContentResult>());
        var fileResult = result as FileContentResult;
        Assert.That(fileResult?.FileContents, Is.EqualTo(media.Data));
        Assert.That(fileResult?.ContentType, Is.EqualTo(media.MimeType)); // Changed MineType to MimeType
        Assert.That(fileResult?.FileDownloadName, Is.EqualTo(Path.GetFileName(media.FileName)));
    }

    [Test]
    public async Task Download_NoAccess_ReturnsForbid()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mockAccesser.Setup(f => f.HasAccessAsync(mediaId, _userId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Download(mediaId);

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task Download_MediaNotFound_ReturnsNotFound()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mockAccesser.Setup(f => f.HasAccessAsync(mediaId, _userId)).ReturnsAsync(true);
        _mockMedia.Setup(f => f.GetMediaByIdAsync(mediaId))
            .ThrowsAsync(new KeyNotFoundException("Media not found"));

        // Act
        var result = await _controller.Download(mediaId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo("Media not found"));
    }

    [Test]
    public async Task Download_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mockAccesser.Setup(f => f.HasAccessAsync(mediaId, _userId)).ReturnsAsync(true);
        _mockMedia.Setup(f => f.GetMediaByIdAsync(mediaId))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.Download(mediaId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }
}