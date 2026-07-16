namespace Govor.Application.Users;

public interface IUserNameExistValidator
{
    Task<bool> IsUsernameExistsAsync(string userName);
}