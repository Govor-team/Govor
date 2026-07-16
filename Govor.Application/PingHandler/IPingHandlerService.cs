namespace Govor.Application.PingHandler;

public interface IPingHandlerService
{
    Task Ping(Guid userId);
}