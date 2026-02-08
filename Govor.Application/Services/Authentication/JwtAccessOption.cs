namespace Govor.Application.Services.Authentication;
public class JwtAccessOption
{
    public string SecretKey {get; set;}
    public int Minutes { get; set; }
}