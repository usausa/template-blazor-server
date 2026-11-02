namespace Template.BlazorServer.Infrastructure.Data;

public sealed class DateTimeTextConverterTests
{
    private static readonly DateTime Utc = new(2026, 11, 2, 9, 30, 15, DateTimeKind.Utc);

    [Fact]
    public void ToDbWritesUtcText()
    {
        // Act
        var text = DateTimeTextConverter.ToDb(Utc);

        // Assert
        Assert.Equal("2026-11-02 09:30:15.0000000", text);
    }

    [Fact]
    public void ToDbConvertsLocalTimeToUtc()
    {
        // Act
        var text = DateTimeTextConverter.ToDb(Utc.ToLocalTime());

        // Assert
        Assert.Equal(DateTimeTextConverter.ToDb(Utc), text);
    }

    [Theory]
    [InlineData("2026-11-02 09:30:15.0000000")]
    [InlineData("2026-11-02 09:30:15")]
    [InlineData("2026-11-02T09:30:15.0000000")]
    public void FromDbReadsUtc(string text)
    {
        // Act
        var value = DateTimeTextConverter.FromDb(text);

        // Assert
        Assert.Equal(Utc, value);
        Assert.Equal(DateTimeKind.Utc, value.Kind);
    }
}
