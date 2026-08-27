namespace Govor.Application.Infrastructure.Common;

public class NowDateTimeProvider : INowDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}