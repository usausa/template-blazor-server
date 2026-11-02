namespace Template.BlazorServer.Components.Dialogs;

using Template.BlazorServer.Domain;
using Template.BlazorServer.Host.Components.Dialogs;

public sealed class NotificationFormValidatorTests
{
    [Fact]
    public void ValidateValidFormReturnsValid()
    {
        // Arrange
        var validator = new NotificationSendDialog.NotificationFormValidator();
        var form = new NotificationSendDialog.NotificationForm { Title = "題", Body = "本文", Link = "data" };

        // Act
        var result = validator.Validate(form);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateEmptyTitleReturnsInvalid()
    {
        // Arrange
        var validator = new NotificationSendDialog.NotificationFormValidator();
        var form = new NotificationSendDialog.NotificationForm { Title = string.Empty, Body = "本文" };

        // Act
        var result = validator.Validate(form);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateTooLongBodyReturnsInvalid()
    {
        // Arrange
        var validator = new NotificationSendDialog.NotificationFormValidator();
        var form = new NotificationSendDialog.NotificationForm { Title = "題", Body = new string('a', Length.NotificationBody + 1) };

        // Act
        var result = validator.Validate(form);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateExternalLinkReturnsInvalid()
    {
        // Arrange
        var validator = new NotificationSendDialog.NotificationFormValidator();
        var form = new NotificationSendDialog.NotificationForm { Title = "題", Body = "本文", Link = "https://example.com" };

        // Act
        var result = validator.Validate(form);

        // Assert
        Assert.False(result.IsValid);
    }
}
