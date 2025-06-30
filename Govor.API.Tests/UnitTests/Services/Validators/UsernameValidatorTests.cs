using Govor.Application.Exceptions.AuthService;
using Govor.Application.Infrastructure.Validators;

namespace Govor.API.Tests.UnitTests.Services.Validators;

[TestFixture]
public class UsernameValidatorTests
{
    private UsernameValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UsernameValidator();
    }

    [TestCase("Иван")]
    [TestCase("Алексей")]
    [TestCase("Ёжик")]
    public void Validate_ValidUsernames_ShouldNotThrow(string username)
    {
        Assert.DoesNotThrow(() => _validator.Validate(username));
    }

    [TestCase("Ivan")] // не кириллица
    [TestCase("123Иван")] // начинается не с буквы
    [TestCase("Иван123")] // содержит цифры
    [TestCase("!@#$")] // спецсимволы
    [TestCase("")] // пусто
    [TestCase("И")] // меньше минимума
    [TestCase("ИванИванИванИванИванИванИванИванИванИванИванИванИван")] // больше максимума (44 символа)
    public void Validate_InvalidUsernames_ShouldThrow(string username)
    {
        Assert.Throws<InvalidUsernameException>(() => _validator.Validate(username));
    }

    [TestCase("Иван", ExpectedResult = true)]
    [TestCase("1234", ExpectedResult = false)]
    public bool TryValidate_ShouldReturnTrueRegardlessOfInput(string username)
    {
        return _validator.TryValidate(username);
    }
}