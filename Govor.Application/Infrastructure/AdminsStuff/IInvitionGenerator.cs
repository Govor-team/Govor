namespace Govor.Application.Infrastructure.AdminsStuff;

public interface IInvitationGenerator
{
    public Task<string> GenerateInvitationCode(DateTime time, int maxUsers, bool isAdmin, string description = "");
}