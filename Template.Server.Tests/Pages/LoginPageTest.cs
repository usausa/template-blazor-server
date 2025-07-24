namespace Template.Server.Pages;

using Template.Components.Storage;

public sealed class LoginPageTest
{
    [Fact]
    public void Test()
    {
        // TODO
        var service = new FileStorage(new FileStorageOptions { Root = "." });
        Assert.NotNull(service);
    }
}
