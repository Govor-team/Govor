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
    
    // Test for Upload action 
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

    
    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }
}