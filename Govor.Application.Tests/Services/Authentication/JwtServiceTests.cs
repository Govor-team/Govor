using System.IdentityModel.Tokens.Jwt;
using AutoFixture;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;
using Govor.Core.Models.Users;
using Microsoft.Extensions.Options;
using Moq;

namespace Govor.Application.Tests.Services.Authentication;

[TestFixture]
public class JwtServiceTests
{
    private Fixture _fixture;
    private Mock<IOptions<JwtOption>> _jwtOptionsMock;
    private Mock<IInvitesService> _invitesServiceMock;
    private IJwtService _jwtService;

    private JwtOption _testJwtOptions;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _testJwtOptions = new JwtOption
        {
            SecretKeу = "THIS IS A TEST SECRET KEY THAT IS LONG ENOUGH", // Ensure key size is sufficient for HMACSHA256
            Hours = 1
        };

        _jwtOptionsMock = new Mock<IOptions<JwtOption>>();
        _jwtOptionsMock.Setup(o => o.Value).Returns(_testJwtOptions);

        _invitesServiceMock = new Mock<IInvitesService>();
        
        _jwtService = new JwtService(_jwtOptionsMock.Object, _invitesServiceMock.Object);
    }

    [Test]
    public async Task GenerateJwtToken_ShouldReturnValidJwtString()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var expectedRole = "User";
        _invitesServiceMock.Setup(s => s.GetRoleAsync(user)).Returns(Task.FromResult(expectedRole));
        // Act 
        var tokenString = await _jwtService.GenerateJwtTokenAsync(user);

        // Assert
        Assert.That(tokenString, Is.Not.Null.And.Not.Empty);
        
        // Attempt to parse the token to ensure it's a JWT
        var handler = new JwtSecurityTokenHandler();
        Assert.DoesNotThrow(() => handler.ReadJwtToken(tokenString));
    }
}