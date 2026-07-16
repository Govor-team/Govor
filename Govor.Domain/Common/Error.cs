namespace Govor.Domain.Common;


public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error Null = new("NULL", "Value cannot be null.");
    public override string ToString() => $"{Code}: {Message}";
}