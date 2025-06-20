using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

[TestFixture]
public class MessagesRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private readonly IObjectValidator<Message> _messageValidator = new MessageValidator();

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: "DbGovor")
            .Options;
    }

    [Test]
    public async Task Given_NotEmptySetDb_When_GetAllMessages_Then_ReturnAllMessages()
    {
        // Arrange
        var random = new Random();
        var messages = _fixture.CreateMany<Message>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();

        // Act 

        var result = await messagesRepository.GetAllAsync();

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(messages.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(messages.Select(u => u.Id)));
        Assert.That(result.Select(u => u.SentAt), Is.EquivalentTo(messages.Select(u => u.SentAt)));
        Assert.That(result.Select(r => r.SenderId), Is.EquivalentTo(messages.Select(u => u.SenderId)));
        Assert.That(result.Select(r => r.RecipientId), Is.EquivalentTo(messages.Select(u => u.RecipientId)));
        Assert.That(result.Select(r => r.RecipientType), Is.EquivalentTo(messages.Select(u => u.RecipientType)));
    }
    
    [Test]
    public void Given_EmptySetDb_When_GetAllMessages_Should_Throw_NotFoundException()
    {
        
    }
}