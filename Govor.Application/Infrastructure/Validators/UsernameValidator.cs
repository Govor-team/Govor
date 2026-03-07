using System.Text.RegularExpressions;
using Govor.Application.Exceptions.AuthService;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Infrastructure.Validators;
using Microsoft.Extensions.Configuration;

namespace Govor.Application.Infrastructure.Validators;

public class UsernameValidator : IUsernameValidator
{
    private readonly Regex _usernameRegex = new(@"^[А-Яа-яЁё]+[А-Яа-яЁё0-9]*$", RegexOptions.Compiled);
    
    private readonly HashSet<string> _blockedExact;
    private readonly List<string> _blockedContains;
    private readonly HashSet<string> _reserved;

    public UsernameValidator(IConfiguration config)
    {
        _blockedExact = config.GetSection("UsernameModeration:BlockedExact")
                            .Get<string[]>()?
                            .Select(Normalize)
                            .ToHashSet()
                        ?? throw new InvalidOperationException("BlockedExact not set");

        _blockedContains = config
                               .GetSection("UsernameModeration:BlockedContains")
                               .Get<string[]>()?
                               .Select(Normalize)
                               .ToList()
                           ?? throw new InvalidOperationException("BlockedContains not set");

        _reserved = config
                        .GetSection("UsernameModeration:Reserved")
                        .Get<string[]>()?
                        .Select(Normalize)
                        .ToHashSet()
                    ?? throw new InvalidOperationException("Reserved not set");
    }
    
    public void Validate(string username)
    {
        if(username.Length < UserValidator.MIN_LENGHT_OF_NAME || username.Length > UserValidator.MAX_LENGHT_OF_NAME)
            throw new InvalidUsernameException($"Username must be between {UserValidator.MIN_LENGHT_OF_NAME} and {UserValidator.MAX_LENGHT_OF_NAME} characters.");

        if (!_usernameRegex.IsMatch(username))
            throw new InvalidUsernameException("The username must be in Cyrillic and start with a letter.");
        
        if (Regex.IsMatch(username, @"(.)\1{4,}"))
            throw new InvalidUsernameException("Too many repeating characters.");
        
        var normalized = Normalize(username);

        if (_reserved.Contains(normalized))
            throw new InvalidUsernameException("This username is reserved.");

        if (_blockedExact.Contains(normalized))
            throw new InvalidUsernameException("This username is not allowed.");

        foreach (var banned in _blockedContains)
        {
            if (normalized.Contains(banned))
                throw new InvalidUsernameException("Username contains prohibited content.");
        }
    }

    public bool TryValidate(string username)
    {
        try
        {
            Validate(username);
            return true;
        }
        catch
        {
            return false;
        }
    }
        
    private static string Normalize(string username)
    {
        return username
            .ToLower()
            .Replace("0", "о")
            .Replace("1", "и")
            .Replace("3", "е")
            .Replace("4", "а")
            .Replace("6", "б")
            .Replace("8", "в");
    }
}