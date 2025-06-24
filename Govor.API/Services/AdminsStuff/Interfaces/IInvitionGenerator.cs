namespace Govor.API.Services.AdminsStuff.Interfaces;

public interface IInvitationGenerator
{
    public Task<string> GenerateInvitationCode(DateTime time, int maxUsers, bool isAdmin, string description = "");
}