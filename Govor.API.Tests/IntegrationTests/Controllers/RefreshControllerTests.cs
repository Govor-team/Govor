using AutoFixture;
using Govor.API.Controllers.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class RefreshControllerTests
{
    private Fixture _fixture;
    private Mock<ILogger<RefreshController>> _mockLogger;
    private Mock<IUserSessionRefresher> _mockSessionRefresher;
    private RefreshController _controller;
    private RefreshTokenRequest _request;
    private RefreshResult _result;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _mockLogger = new Mock<ILogger<RefreshController>>();
        _mockSessionRefresher = new Mock<IUserSessionRefresher>();
        
        _request = _fixture.Create<RefreshTokenRequest>();
        _result = _fixture.Create<RefreshResult>();
        
        _controller = new RefreshController(_mockLogger.Object, _mockSessionRefresher.Object);
    }
    
    // Tests for Refresh action
    [Test]
    public async Task Refresh_ValidRequest_ReturnsOkResult()
    {
        // Arrange 
        _mockSessionRefresher.Setup(f => f.RefreshTokenAsync(_request.RefreshToken))
            .ReturnsAsync(_result);
        
        // Act 
        var result = await _controller.Refresh(_request);
        
        //Assert 
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;

        var response = okResult?.Value as RefreshTokenResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.AccessToken, Is.EqualTo(_result.accessToken));
        Assert.That(response.RefreshToken, Is.EqualTo(_result.refreshToken));
    }
    
    [Test]
    public async Task Refresh_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Sample error");

        // Act
        var result = await _controller.Refresh(_request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Refresh_InvalidRefreshToken_ReturnsBadRequest()
    {
        // Arrange
        _request.RefreshToken = string.Empty;

        // Act
        var result = await _controller.Refresh(_request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult?.Value;
        Assert.That(response, Is.EqualTo("Refresh token cant be empty."));
    }
    
    [Test]
    public async Task Refresh_UnauthorizedAccessException_ReturnsUnauthorizedRequest()
    {
        // Arrange
        _mockSessionRefresher.Setup(f => f.RefreshTokenAsync(_request.RefreshToken))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.Refresh(_request);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorized = result as UnauthorizedObjectResult;
        var response = unauthorized?.Value;
        Assert.That(response, Is.EqualTo("Invalid refresh token"));
    }
    
    [Test]
    public async Task Refresh_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        _mockSessionRefresher.Setup(f => f.RefreshTokenAsync(_request.RefreshToken))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await _controller.Refresh(_request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}