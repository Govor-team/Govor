using AutoFixture;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

[TestFixture]
public class AdminsRepositoryTests
{ 
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private int _testIteration = 0;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        
        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(AdminsRepositoryTests)}_{_testIteration}")
            .Options;
    }
    
    [Test]
    public async Task Given_NotEmptyDbSet_When_GetAllAsync_Then_Admins_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var admins = _fixture.CreateMany<Admin>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context);
        
        context.AddRange(admins);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetAllAsync();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(admins.Count()));
        Assert.That(result.Select(r => r.UserId), Is.EquivalentTo(admins.Select(r => r.UserId)));
    }
}