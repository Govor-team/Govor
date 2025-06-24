using AutoFixture;
using Govor.API.Services.Authentication;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Core.Repositories.Admins;
using Govor.Core.Repositories.Invaites;
using Moq;

namespace Govor.API.Tests.UnitTests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Fixture _fixture;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IAdminsRepository> _adminsRepositoryMock;
    
    private IAccountService _accountService;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _adminsRepositoryMock = new Mock<IAdminsRepository>();
        
        _accountService = new AuthService(
            _usersRepositoryMock.Object,
            _jwtServiceMock.Object,
            _passwordHasherMock.Object,
            _adminsRepositoryMock.Object
            );
    }
    
    [Test]
    public void Given_ExistUser_When_Register_Should_Throw_UserAlreadyExistsException()
    {
        // Arrange 
        _usersRepositoryMock.Setup(r => r.ExistsUsernameAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        // Act & Assert 
        Assert.ThrowsAsync<UserAlreadyExistException>(async () => await _accountService.RegistrationAsync(
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Invitation>()));
    }

    [Test]
    public void Given_NotExistUser_When_Register_Should_Dont_Throw_UserNotRegisteredException()
    {
        // Arrange 
        _usersRepositoryMock.Setup(r => r.ExistsUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
        
        // Act & Assert 
        Assert.DoesNotThrowAsync(async () => await _accountService.RegistrationAsync(
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Invitation>()));
    }
    
    [Test]
    public void Given_WrongPassword_When_Login_Should_Throw_LoginUserException()
    {
        // Arrange 
        _usersRepositoryMock.Setup(r => r.ExistsUsernameAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(),
            It.IsAny<string>())).Returns(false);
        
        _usersRepositoryMock.Setup(u => u.FindByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(() => _fixture.Create<User>());
        
        // Act & Assert 
        Assert.ThrowsAsync<LoginUserException>(async () => await _accountService.LoginAsync(
            _fixture.Create<string>(), _fixture.Create<string>()));
    }
    
    [Test]
    public void Given_NotExistUser_When_Login_Should_Throw_UserNotRegisteredException()
    {
        // Arrange 
        _usersRepositoryMock.Setup(r => r.ExistsUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
        
        // Act & Assert 
        Assert.ThrowsAsync<UserNotRegisteredException>(async () => await _accountService.LoginAsync(
            _fixture.Create<string>(), _fixture.Create<string>()));
    }
}