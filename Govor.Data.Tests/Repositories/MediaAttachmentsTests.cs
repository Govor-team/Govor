using AutoFixture;
using Govor.Domain.Infrastructure.Validators;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain;
using Govor.Domain.Repositories;
using Govor.Domain.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain.Tests.Repositories;

[TestFixture]
public class MediaAttachmentsTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private readonly IObjectValidator<MediaAttachments> _validator = new MediaAttachmentsValidator();
    private int _testIteration = 0;

    [SetUp]
    public void SetUp()
    {
        _testIteration += 1;
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(MediaAttachmentsTests)}_{_testIteration}")
            .Options;
    }
    
    [Test]
    public async Task Given_NotEmptySetDb_When_GetAll_Then_ReturnAllAttachments()
    {
        // Arrange
        var random = new Random();
        var attachments = _fixture.CreateMany<MediaAttachments>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var attachmentsRepository = new MediaAttachmentsRepository(context, _validator);

        context.MediaAttachments.AddRange(attachments);
        await context.SaveChangesAsync();

        // Act 

        var result = await attachmentsRepository.GetAllAsync();

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(attachments.Count));
        Assert.That(result, Is.EquivalentTo(attachments));
    }
    
    [Test]
    public void Given_EmptySetDb_When_GetAll_Should_Throw_NotFoundException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var mediaAttachments = new MediaAttachmentsRepository(context, _validator);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await mediaAttachments.GetAllAsync());
    }
    
    [Test]
    public async Task Given_ValidMessageId_When_GetAllByMessageId_Then_ReturnAllAttachments()
    {
        // Arrange
        var random = new Random();
        var attachments = _fixture.CreateMany<MediaAttachments>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var attachmentsRepository = new MediaAttachmentsRepository(context, _validator);

        context.MediaAttachments.AddRange(attachments);
        await context.SaveChangesAsync();

        // Act 
        var result = await attachmentsRepository.GetAllByMessageId(attachments.First().MessageId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First(), Is.EqualTo(attachments.First()));
    }

    [Test]
    public async Task Given_InvalidMessageId_When_GetAllByMessageId_Should_Throw_NotFoundByKeyException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var attachmentsRepository = new MediaAttachmentsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await attachmentsRepository.GetAllByMessageId(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidAttachmentsId_When_FindByIdAsync_Then_Returns_Attachment()
    {
        // Arrange 
        var attachment = _fixture.Create<MediaAttachments>();
        
        await using var context = new GovorDbContext(_options);
        var attachmentsRepository = new MediaAttachmentsRepository(context, _validator);

        context.MediaAttachments.Add(attachment);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await attachmentsRepository.FindByIdAsync(attachment.Id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(attachment));
    }
    
    [Test]
    public async Task Given_InvalidAttachmentsId_When_FindByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var attachmentsRepository = new MediaAttachmentsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await attachmentsRepository.FindByIdAsync(_fixture.Create<Guid>()));
    }
    
    
    [Test]
    public async Task Given_ValidAttachments_When_AddAsync_Then_AttachmentsAdded()
    {
        // Arrange
        var attachments = _fixture.Create<MediaAttachments>();
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MediaAttachmentsRepository(context, _validator);
    
        // Act 
        await messagesRepository.AddAsync(attachments);
        
        // Assert 
        Assert.That(context.MediaAttachments.Count, Is.EqualTo(1));
        Assert.That(context.MediaAttachments.First(), Is.EqualTo(attachments));
    }
    
    [Test]
    public async Task Given_InvalidAttachments_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistAttachment_When_Exist_Then_ReturnTrue()
    {
        // Arrange
        var attachments = _fixture.Create<MediaAttachments>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);
        
        context.MediaAttachments.Add(attachments);
        await context.SaveChangesAsync();
        
        // Act 
        var result = repository.Exist(attachments);
        var result2 = repository.Exist(attachments.Id);
        
        // Assert 
        Assert.That(result, Is.True);
        Assert.That(result2, Is.True);
    }

    [Test]
    public async Task Given_NotExistMessage_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var attachments = _fixture.Create<MediaAttachments>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);

        
        // Act 
        var result = repository.Exist(attachments);
        var result2 = repository.Exist(attachments.Id);
        
        // Assert 
        Assert.That(result, Is.False);
        Assert.That(result2, Is.False);
    }
    
    [Test]
    public async Task Given_NotEqualMessage_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var attachments = _fixture.Create<MediaAttachments>();
        var attachments2 = _fixture.Create<MediaAttachments>();
        attachments2.Id = attachments.Id;
        
        await using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);
        
        context.MediaAttachments.Add(attachments);
        await context.SaveChangesAsync();
        
        // Act 
        var result = repository.Exist(attachments2);
        
        // Assert 
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Given_InvalidMessage_When_Exist_Should_Throw_InvalidObjectException()
    {
        // Arrange 
        var attachments = _fixture.Create<MediaAttachments>();
        attachments.MessageId = Guid.Empty;
        
        await using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<MediaAttachments>>(() => repository.Exist(attachments));
    }

    [Test]
    public void Given_NullMessage_When_Exist_Should_Throw_InvalidObjectException()
    {
        using var context = new GovorDbContext(_options);
        var repository = new MediaAttachmentsRepository(context, _validator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<MediaAttachments>>(() => repository.Exist(default(MediaAttachments)));
    }
}