using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

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
}