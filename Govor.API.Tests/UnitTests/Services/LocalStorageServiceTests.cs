using AutoFixture;
using Govor.API.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Govor.API.Tests.UnitTests.Services;

[TestFixture]
public class LocalStorageServiceTests
{
    private Fixture _fixture;
    private IStorageService _service;
    private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private string _tempDirectory;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _webHostEnvironmentMock.Setup(w => w.ContentRootPath).Returns(_tempDirectory);

        _service = new LocalStorageService(_webHostEnvironmentMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
    
    [Test]
    public async Task Given_ValidDataAndFileName_When_SaveAsync_Then_FileIsSaved()
    {
        // Arrange
        var data = _fixture.CreateMany<byte>(1000).ToArray();
        var fileName = _fixture.Create<string>() + ".txt";
        var saveDate = DateTime.UtcNow;

        // Act
        var url = await _service.SaveAsync(data, fileName);

        // Assert
        Assert.That(url, Is.Not.Null);

        // _tempDirectory/uploads/yyyy/MM/url
        var datePath = saveDate.ToString("yyyy/MM");
        var expectedFilePath = Path.Combine(_tempDirectory, "uploads", url);
        Assert.That(File.Exists(expectedFilePath), Is.True, $"Файл не найден по пути: {expectedFilePath}");
    }
    
    [Test]
    public void Given_EmptyName_When_SaveAsync_Should_Throw_ArgumentException()
    {
        // Arrange 
        var data = _fixture.CreateMany<byte>(1000).ToArray();
        
        // Act & Assert 
        Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveAsync(data, string.Empty));
    }

    [Test]
    public void Given_EmptyData_When_SaveAsync_Should_Throw_ArgumentException()
    {
        // Arrange 
        var name = _fixture.Create<string>();
        
        // Act & Assert 
        Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveAsync(default, name));
    }
}