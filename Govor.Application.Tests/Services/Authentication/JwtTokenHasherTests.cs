using AutoFixture;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;

namespace Govor.Application.Tests.Services.Authentication;

[TestFixture]
public class JwtTokenHasherTests
{
    private IJwtTokenHasher _jwtTokenHasher;
    private Fixture _fixture;
    
    [SetUp]
    public void StarUp()
    {
        _fixture = new Fixture();
        //_jwtTokenHasher = new JwtTokenHasher();
    }
    
    [Test]
    public void Given_Token_When_Hash_Then_Hash_And_Verify_Then_Result_Should_Be_True()
    {
        // Arrange
        string token = _fixture.Create<string>();
        
        // Act 
        string hash = _jwtTokenHasher.HashToken(token);
        
        var result = _jwtTokenHasher.VerifyToken(token, hash);
        
        // Assert 
        Assert.That(hash, Is.Not.EqualTo(token));
        Assert.That(result, Is.True);
    }

    [Test]
    public void Given_Token_NotToken_When_Hash_Should_Not_Be_True_Then_Result_Should_Be_False()
    {
        // Arrange 
        string token = _fixture.Create<string>();
        string notToken = _fixture.Create<string>();
        
        // Act 
        string hash = _jwtTokenHasher.HashToken(token);
        
        var result = _jwtTokenHasher.VerifyToken(notToken, hash);
        
        // Assert 
        Assert.That(result, Is.False);
    }
}