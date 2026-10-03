using Govor.Domain;
using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Messages;

public static class ChatAccess
{
    public static Task<bool> HasChatAccessAsync(this GovorDbContext context,
        Guid userId, Guid chatId, RecipientType type)
    {
        if (userId == Guid.Empty || chatId == Guid.Empty)
            return Task.FromResult(false);

        return type switch
        {
            RecipientType.User => context.PrivateChats.AnyAsync(c => c.Id == chatId &&
                (c.UserAId == userId || c.UserBId == userId)),
            RecipientType.Group => context.GroupMemberships.AnyAsync(m =>
                m.GroupId == chatId && m.UserId == userId && !m.IsBanned),
            _ => Task.FromResult(false)
        };
    }
}
