namespace Template.BlazorServer.Domain;

public sealed class NotificationLinkTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("data")]
    [InlineData("data?name=Item%201")]
    [InlineData("ignite/buttons")]
    public void IsValidAcceptsRelativePath(string? link)
    {
        // Act
        var valid = NotificationLink.IsValid(link);

        // Assert
        Assert.True(valid);
    }

    [Theory]
    [InlineData("/data")]
    [InlineData("//example.com")]
    [InlineData("\\\\example.com")]
    [InlineData("https://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData(" data")]
    [InlineData("データ")]
    public void IsValidRejectsOtherLink(string link)
    {
        // Act
        var valid = NotificationLink.IsValid(link);

        // Assert
        Assert.False(valid);
    }

    [Fact]
    public void IsValidRejectsTooLongLink()
    {
        // Act
        var valid = NotificationLink.IsValid(new string('a', Length.NotificationLink + 1));

        // Assert
        Assert.False(valid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CanOpenRejectsEmptyLink(string? link)
    {
        // Act
        var open = NotificationLink.CanOpen(link);

        // Assert
        Assert.False(open);
    }
}
