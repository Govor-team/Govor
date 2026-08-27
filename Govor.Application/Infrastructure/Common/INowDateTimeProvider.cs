namespace Govor.Application.Infrastructure.Common;

public interface INowDateTimeProvider
{
    DateTime Now { get; }
}