using Govor.ConsoleClient.Services.Interfaces;
using Moq;

namespace Govor.ConsoleClient.Tests;

[TestFixture]
public class AppTests
{
    private Mock<IInputPipeline> _inputPipelineMock = null!;
    private Mock<ILogger> _loggerMock = null!;
    private App _app = null!;

    [SetUp]
    public void Setup()
    {
        _inputPipelineMock = new Mock<IInputPipeline>();
        _loggerMock = new Mock<ILogger>();
        
        _inputPipelineMock.Setup(f => f.ProcessInputAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        
        _app = new App(_inputPipelineMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task RunAsync_PrintsTitleOnce()
    {
        using var sr = new StringReader("exit");
        Console.SetIn(sr);

        await _app.RunAsync();

        _loggerMock.Verify(l => l.Title(It.Is<string>(s => s.Contains("Говор"))), Times.Once);
    }

    [Test]
    public async Task RunAsync_CallsInputPipeline_ForEachInput()
    {
        var inputText = "hello\n/command\nexit";
        using var sr = new StringReader(inputText);
        Console.SetIn(sr);

        await _app.RunAsync();

        _inputPipelineMock.Verify(p => p.ProcessInputAsync("hello"), Times.Once);
        _inputPipelineMock.Verify(p => p.ProcessInputAsync("/command"), Times.Once);
    }

    [Test]
    public async Task RunAsync_StopsOnExit()
    {
        using var sr = new StringReader("exit");
        Console.SetIn(sr);

        await _app.RunAsync();

        // Убедимся, что ProcessInputAsync вообще не вызывался
        _inputPipelineMock.Verify(p => p.ProcessInputAsync(It.IsAny<string>()), Times.Never);
    }
}