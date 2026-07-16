using Govor.Application.Messages.Parameters;
using Govor.Domain;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Messages;

public class MessageRemovingService : IMessageRemovingService
{
    private readonly GovorDbContext _govorDbContext;
    private readonly ILogger<MessageRemovingService> _logger;

    public MessageRemovingService
    (GovorDbContext govorDbContext, 
        ILogger<MessageRemovingService> logger)
    {
        _govorDbContext = govorDbContext;
        _logger = logger;
    }

    public Task<DeleteMessageResult> DeleteMessageAsync(DeleteMessage deleteParams)
    {
        throw new NotImplementedException();
    }
}