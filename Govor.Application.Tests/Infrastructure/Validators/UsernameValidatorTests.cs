using Govor.Application.Exceptions.AuthService;
using Govor.Application.Infrastructure.Validators;
using Microsoft.Extensions.Configuration;

namespace Govor.Application.Tests.Infrastructure.Validators;

[TestFixture]
public class UsernameValidatorTests
{
    private UsernameValidator _validator;

    [SetUp]
    public void SetUp()
    {
        var configData = new Dictionary<string, string?>
        {
            ["UsernameModeration:BlockedExact:0"] = "админ",
            ["UsernameModeration:BlockedExact:1"] = "модератор",

            ["UsernameModeration:BlockedContains:0"] = "гитлер",
            ["UsernameModeration:BlockedContains:1"] = "наци",

            ["UsernameModeration:Reserved:0"] = "логин",
            ["UsernameModeration:Reserved:1"] = "регистрация"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _validator = new UsernameValidator(config);
    }

    [TestCase("Иван")]
    [TestCase("Алексей")]
    [TestCase("Ёжик")]
    [TestCase("Иван123")] // содержит цифры
    public void Validate_ValidUsernames_ShouldNotThrow(string username)
    {
        Assert.DoesNotThrow(() => _validator.Validate(username));
    }

    [TestCase("Ivan")] // не кириллица
    [TestCase("123Иван")] // начинается не с буквы
    [TestCase("!@#$")] // спецсимволы
    [TestCase("")] // пусто
    [TestCase("И")] // меньше минимума
    [TestCase("ИванИванИванИванИванИванИванИванИванИванИванИванИван")] // больше максимума (44 символа)
    public void Validate_InvalidUsernames_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Ааааааа")] 
    [TestCase("Бbbbb")] 
    public void Validate_RepeatingCharacters_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Админ")]
    [TestCase("Модератор")]
    public void Validate_BlockedExact_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Гитлер123")]
    [TestCase("Нацист")]
    public void Validate_BlockedContains_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Логин")]
    [TestCase("Регистрация")]
    public void Validate_ReservedNames_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Г1тлер")]
    public void Validate_Normalization_ShouldDetectBlockedWord(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Иван", ExpectedResult = true)]
    [TestCase("1234", ExpectedResult = false)]
    [TestCase("Админ", ExpectedResult = false)]
    [TestCase("Гитлер123", ExpectedResult = false)]
    public bool TryValidate_ShouldReturnExpectedResult(string username)
    {
        return _validator.TryValidate(username);
    }
}