namespace Govor.Contracts.Responses;

public class RefreshTokenResponse
{
    public string RefreshToken { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
}