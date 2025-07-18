using AutoFixture;
using Govor.API.Controllers;
using Govor.Application.Exceptions.AuthService;
using Govor.Application.Exceptions.InvitesService;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Contracts.Requests;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Fixture _fixture;
    private Mock<IAccountService> _accountServiceMock;
    private Mock<IInvitesService> _invitesServiceMock;
    private Mock<ILogger<AuthController>> _loggerMock;
    private Mock<IUserSessionOpener> _userSessionOpenerMock;
    private AuthController _controller;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accountServiceMock = new Mock<IAccountService>();
        _invitesServiceMock = new Mock<IInvitesService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _userSessionOpenerMock = new Mock<IUserSessionOpener>();
        
        _controller = new AuthController(
            _accountServiceMock.Object,
            _invitesServiceMock.Object,
            _userSessionOpenerMock.Object,
            _loggerMock.Object
        );
    }
    
    // Tests for Register action
    [Test]
    public async Task Register_ValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var request = _fixture.Create<RegistrationRequest>();
        var invitation = _fixture.Create<Invitation>();
        var token = _fixture.Create<string>();
        
        var user = _fixture.Build<User>()
            .With(x => x.Username).Create();
        
        _invitesServiceMock.Setup(s => s.ValidateAsync(request.InviteLink)).ReturnsAsync(invitation);

       
        _accountServiceMock.Setup(l => l.RegistrationAsync(request.Name, request.Password, invitation))
            .ReturnsAsync(user);
        
        _userSessionOpenerMock.Setup(f => f.OpenSessionAsync(user, request.DeviceInfo))
            .ReturnsAsync(token);

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        dynamic value = okResult.Value;
        Assert.That((string)value.GetType().GetProperty("token").GetValue(value, null), Is.EqualTo(token));
    }
    
    [Test]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = _fixture.Create<RegistrationRequest>();
        _controller.ModelState.AddModelError("Error", "Sample error");

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Register_InviteLinkInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = _fixture.Create<RegistrationRequest>();
        _invitesServiceMock.Setup(s => s.ValidateAsync(request.InviteLink)).ThrowsAsync(new InviteLinkInvalidException(request.InviteLink));

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var notFoundObjectResult = result as BadRequestObjectResult;
        Assert.That(notFoundObjectResult.Value, Is.EqualTo("Invite link invalid."));
    }

    [Test]
    public async Task Register_UserAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var request = _fixture.Create<RegistrationRequest>();
        var invitation = _fixture.Create<Invitation>();
        _invitesServiceMock.Setup(s => s.ValidateAsync(request.InviteLink)).ReturnsAsync(invitation);
        _accountServiceMock.Setup(s => s.RegistrationAsync(request.Name, request.Password, invitation))
            .ThrowsAsync(new UserAlreadyExistException(request.Name));

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Registration failed: user already exists."));
    }
    
    [Test]
    public async Task Register_AccountServiceThrowsGenericException_ReturnsStatusCode500()
    {
        // Arrange
        var request = _fixture.Create<RegistrationRequest>();
        var invitation = _fixture.Create<Invitation>();
        _invitesServiceMock.Setup(s => s.ValidateAsync(request.InviteLink)).ReturnsAsync(invitation);
        _accountServiceMock.Setup(s => s.RegistrationAsync(request.Name, request.Password, invitation))
            .ThrowsAsync(new System.Exception("Generic error"));
    
        // Act
        var result = await _controller.Register(request);
    
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("An unexpected error occurred. Please try again later."));
    }
    
    // Tests for Login action
    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange 
        var loginRequest = _fixture.Create<LoginRequest>();
        var token = _fixture.Create<string>();
        
        var user = _fixture.Build<User>()
            .With(x => x.Username).Create();
       
        _accountServiceMock.Setup(l => l.LoginAsync(loginRequest.Name, loginRequest.Password)).ReturnsAsync(user);
        
        _userSessionOpenerMock.Setup(f => f.OpenSessionAsync(user, loginRequest.DeviceInfo))
            .ReturnsAsync(token);
        
        // Act 
        var result = await _controller.Login(loginRequest);
        // Assert 
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        dynamic value = okResult.Value;
        Assert.That((string)value.GetType().GetProperty("token").GetValue(value, null), Is.EqualTo(token));
    }
    
    [Test]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        _controller.ModelState.AddModelError("Error", "Sample error");

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Login_UserNotRegistered_ReturnsBadRequest()
    {
        // Arrange 
        var request = _fixture.Create<LoginRequest>();
        _accountServiceMock.Setup(l => l.LoginAsync(request.Name, request.Password)).Throws(new UserNotRegisteredException(request.Name));
        
        // Act 
        var result = await _controller.Login(request);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Login failed: user does not exist."));
    }
    
    [Test]
    public async Task Login_PasswordIsIncorrect_ReturnsBadRequest()
    {
        // Arrange 
        var request = _fixture.Create<LoginRequest>();
        _accountServiceMock.Setup(l => l.LoginAsync(request.Name, request.Password)).Throws(new LoginUserException());
        
        // Act 
        var result = await _controller.Login(request);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Login failed: username or password is incorrect."));
    }
    
    [Test]
    public async Task Login_AccountServiceThrowsGenericException_ReturnsStatusCode500()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        _accountServiceMock.Setup(s => s.LoginAsync(request.Name, request.Password))
            .ThrowsAsync(new System.Exception("Generic error"));
    
        // Act
        var result = await _controller.Login(request);
    
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("An unexpected error occurred. Please try again later."));
    }
    
    [TearDown]
    public void TearDown()
    {
        (_controller as IDisposable)?.Dispose();
    }
}