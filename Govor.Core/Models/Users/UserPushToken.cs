using Microsoft.EntityFrameworkCore;

namespace Govor.Core.Models.Users;
public class UserPushToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid? UserSessionId { get; set; }

    public string Token { get; set; } = string.Empty;        // FCM/APNs 
    public string Provider { get; set; } = "FCM";            // FCM | APNs | Web | Huawei
    public string Platform { get; set; } = string.Empty;     // android | ios | web

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }

    public bool IsActive { get; set; } = true;
}