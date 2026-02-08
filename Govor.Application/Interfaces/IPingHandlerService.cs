namespace Govor.Application.Interfaces;

public interface IPingHandlerService
{
    Task Ping(Guid userId);
}