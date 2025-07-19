using Govor.ConsoleClient.Commands;
using Govor.ConsoleClient.Services.Implementations;
using Govor.ConsoleClient.Services.Interfaces;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Tests.Services.Implementations;

[TestFixture]
public class InputPipelineTests
{
    private Mock<ICommandDispatcher> _commandDispatcherMock = null!;
    private Mock<ILogger> _loggerMock = null!;
    private InputPipeline _inputPipeline = null!;

    [SetUp]
    public void SetUp()
    {
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _loggerMock = new Mock<ILogger>();

        _inputPipeline = new InputPipeline(_commandDispatcherMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ProcessInputAsync_EmptyOrWhitespaceInput_DoesNothing()
    {
        // Act
        await _inputPipeline.ProcessInputAsync(null!);
        await _inputPipeline.ProcessInputAsync("");
        await _inputPipeline.ProcessInputAsync("   ");

        // Assert 
        _commandDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ProcessInputAsync_CommandInput_DispatchesCommandAndResetsActiveCommand()
    {
        // Arrange 
        var commandMock = new Mock<ICommand>();
        _commandDispatcherMock
            .Setup(d => d.DispatchAsync("testcmd"))
            .ReturnsAsync(commandMock.Object);
        
        // Act 
        await _inputPipeline.ProcessInputAsync("/testcmd");

        // Assert
        _commandDispatcherMock.Verify(d => d.DispatchAsync("testcmd"), Times.Once);
        _loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ProcessInputAsync_CommandInput_IfInteractiveCommand_SetsActiveCommand()
    {
        // Arrange 
        var interactiveCommandMock = new Mock<IInteractiveCommand>();
        interactiveCommandMock.SetupGet(c => c.IsCompleted).Returns(false);
        _commandDispatcherMock
            .Setup(d => d.DispatchAsync("interactive"))
            .ReturnsAsync(interactiveCommandMock.Object);
        
        // Act 
        await _inputPipeline.ProcessInputAsync("/interactive");
        
        // Assert 
        var activeCommandField = typeof(InputPipeline).GetField("_activeCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var activeCommandValue = activeCommandField!.GetValue(_inputPipeline);
        Assert.That(activeCommandValue, Is.SameAs(interactiveCommandMock.Object));
    }

    [Test]
    public async Task ProcessInputAsync_NonCommandInput_WithActiveInteractiveCommand_CallsHandleInputAsync()
    {   
        // Arrange 
        var interactiveCommandMock = new Mock<IInteractiveCommand>();
        interactiveCommandMock.SetupGet(c => c.IsCompleted).Returns(false);
        interactiveCommandMock.Setup(c => c.HandleInputAsync("user input")).Returns(Task.CompletedTask);
        
        var activeCommandField = typeof(InputPipeline).GetField("_activeCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        activeCommandField!.SetValue(_inputPipeline, interactiveCommandMock.Object);

        // Act 
        await _inputPipeline.ProcessInputAsync("user input");
            
        // Assert 
        interactiveCommandMock.Verify(c => c.HandleInputAsync("user input"), Times.Once);
    }

    [Test]
    public async Task ProcessInputAsync_NonCommandInput_WithActiveInteractiveCommand_ResetsActiveCommand_WhenCompleted()
    {
        // Arrange 
        var interactiveCommandMock = new Mock<IInteractiveCommand>();
        interactiveCommandMock.SetupSequence(c => c.IsCompleted)
            .Returns(false)
            .Returns(true);
        interactiveCommandMock.Setup(c => c.HandleInputAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var activeCommandField = typeof(InputPipeline).GetField("_activeCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        activeCommandField!.SetValue(_inputPipeline, interactiveCommandMock.Object);

        // Act & Assert 1
        await _inputPipeline.ProcessInputAsync("input1");
        Assert.That(interactiveCommandMock.Object, Is.SameAs(activeCommandField.GetValue(_inputPipeline)));

        // Act & Assert 2
        await _inputPipeline.ProcessInputAsync("input2");
        Assert.That(activeCommandField.GetValue(_inputPipeline), Is.Null);
    }

    [Test]
    public async Task ProcessInputAsync_NonCommandInput_WithoutActiveCommand_LogsInfo()
    {
        // Act
        await _inputPipeline.ProcessInputAsync("just text");

        // Assert 
        _loggerMock.Verify(l => l.Info("Введите /help, чтобы узнать доступные команды!"), Times.Once);
    }
}
