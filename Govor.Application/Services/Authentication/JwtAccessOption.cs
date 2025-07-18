namespace Govor.Application.Services.Authentication;
public class JwtAccessOption
{
    public string SecretKeу {get; set;}
    public int Minutes { get; set; }
}