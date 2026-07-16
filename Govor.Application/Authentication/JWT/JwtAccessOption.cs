namespace Govor.Application.Authentication.JWT;
public class JwtAccessOption
{
    public string SecretKey {get; set;}
    public int Minutes { get; set; }
}