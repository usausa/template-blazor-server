namespace Template.BlazorServer.Components.Pages.TextResource;

using System.Globalization;

using Template.BlazorServer.Domain;
using Template.BlazorServer.Host.Components.Pages.TextResource;

public sealed class LocaleFormValidatorTests
{
    [Fact]
    public void ValidateValidFormReturnsValid()
    {
        // Arrange
        var validator = new LocalePage.LocaleFormValidator();
        var form = new LocalePage.LocaleForm { Name = "Name" };

        // Act
        var result = validator.Validate(form);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ja-JP", "名前を入力してください。")]
    [InlineData("en-US", "Please enter a name.")]
    public void ValidateEmptyNameUsesCultureOfValidation(string name, string expected)
    {
        // Arrange
        var validator = new LocalePage.LocaleFormValidator();
        var form = new LocalePage.LocaleForm();
        var original = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
        try
        {
            // Act
            var result = validator.Validate(form);

            // Assert
            Assert.Equal(expected, Assert.Single(result.Errors).ErrorMessage);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Theory]
    [InlineData("ja-JP", "名前は50文字以内で入力してください。")]
    [InlineData("en-US", "Name must be 50 characters or less.")]
    public void ValidateTooLongNameUsesCultureOfValidation(string name, string expected)
    {
        // Arrange
        var validator = new LocalePage.LocaleFormValidator();
        var form = new LocalePage.LocaleForm { Name = new string('a', Length.Name + 1) };
        var original = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
        try
        {
            // Act
            var result = validator.Validate(form);

            // Assert
            Assert.Equal(expected, Assert.Single(result.Errors).ErrorMessage);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }
}
