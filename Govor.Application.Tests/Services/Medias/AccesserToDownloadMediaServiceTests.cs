using Govor.Application.Services.Medias;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Data;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Tests.Services.Medias;

[TestFixture]
public class AccesserToDownloadMediaServiceTests
{
    private GovorDbContext _dbContext = null!;
    private AccesserToDownloadMediaService _accesser = null!;
    private Guid _userId;
    private Guid _otherUserId;
    private Guid _groupId;
    private Guid _mediaFileId;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GovorDbContext(options);
        _accesser = new AccesserToDownloadMediaService(_dbContext);

        _userId = Guid.NewGuid();
        _otherUserId = Guid.NewGuid();
        _groupId = Guid.NewGuid();
        _mediaFileId = Guid.NewGuid();

        // Seed message from user to other user
        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = _userId,
            RecipientId = _otherUserId,
            RecipientType = RecipientType.User
        };


        var media = new MediaFile
        {
            Id = _mediaFileId,
            Url = "/media/test.png",
            MineType = "image/png",
            MediaType = MediaType.Image,
            UploaderId = _userId,
            DateCreated = DateTime.UtcNow
        };

        var attachment = new MediaAttachments
        {
            Id = Guid.NewGuid(),
            MediaFileId = _mediaFileId,
            MessageId = message.Id,
            Message = message,
            MediaFile = media
        };

        await _dbContext.Messages.AddAsync(message);
        await _dbContext.MediaFiles.AddAsync(media);
        await _dbContext.MediaAttachments.AddAsync(attachment);
        await _dbContext.SaveChangesAsync();
    }

    [Test]
    public async Task HasAccessAsync_ReturnsTrue_ForSender()
    {
        var result = await _accesser.HasAccessAsync(_mediaFileId, _userId);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasAccessAsync_ReturnsTrue_ForRecipient()
    {
        var result = await _accesser.HasAccessAsync(_mediaFileId, _otherUserId);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasAccessAsync_ReturnsFalse_ForUnrelatedUser()
    {
        var unrelatedUserId = Guid.NewGuid();
        var result = await _accesser.HasAccessAsync(_mediaFileId, unrelatedUserId);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasAccessAsync_ReturnsTrue_ForGroupMember()
    {
        var groupMediaId = Guid.NewGuid();

        var groupMessage = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = _userId,
            RecipientId = _groupId,
            RecipientType = RecipientType.Group
        };

        var media = new MediaFile
        {
            Id = groupMediaId,
            Url = "/media/group.png",
            MineType = "image/png",
            MediaType = MediaType.Image,
            UploaderId = _userId,
            DateCreated = DateTime.UtcNow
        };

        var attachment = new MediaAttachments
        {
            Id = Guid.NewGuid(),
            MediaFileId = groupMediaId,
            MessageId = groupMessage.Id,
            Message = groupMessage,
            MediaFile = media
        };

        var membership = new GroupMembership
        {
            Id = Guid.NewGuid(),
            GroupId = _groupId,
            UserId = _otherUserId
        };

        await _dbContext.Messages.AddAsync(groupMessage);
        await _dbContext.MediaFiles.AddAsync(media);
        await _dbContext.MediaAttachments.AddAsync(attachment);
        await _dbContext.GroupMemberships.AddAsync(membership);
        await _dbContext.SaveChangesAsync();

        var result = await _accesser.HasAccessAsync(groupMediaId, _otherUserId);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasAccessAsync_ReturnsFalse_IfMediaNotAttached()
    {
        var result = await _accesser.HasAccessAsync(Guid.NewGuid(), _userId);
        Assert.That(result, Is.False);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}
