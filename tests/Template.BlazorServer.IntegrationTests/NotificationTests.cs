namespace Template.BlazorServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Smart.Data;

using Template.BlazorServer.Accessors;
using Template.BlazorServer.Host.Application.Context;
using Template.BlazorServer.Host.Workers;
using Template.BlazorServer.Infrastructure.Security;
using Template.BlazorServer.Services;

public sealed class NotificationTests
{
    private const string Admin = "admin";

    private const string User = "user";

    [Fact]
    public async Task SendToAllReachesEveryAccount()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        await AddAccountAsync(services, User);
        var service = services.GetRequiredService<NotificationService>();

        // Act
        int count;
        using (Begin(services, Admin))
        {
            count = await service.SendAsync(null, "題", "本文", null);
        }

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(1, await service.CountUnreadAsync(Admin, TestContext.Current.CancellationToken));
        Assert.Equal(1, await service.CountUnreadAsync(User, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SendToUserReachesOnlyUser()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        await AddAccountAsync(services, User);
        var service = services.GetRequiredService<NotificationService>();
        var notified = new List<string?>();
        services.GetRequiredService<NotificationChangeNotifier>().Changed += (_, e) => notified.Add(e.UserName);

        // Act
        int count;
        int unknown;
        using (Begin(services, Admin))
        {
            count = await service.SendAsync(User, "題", "本文", "data");
            unknown = await service.SendAsync("nobody", "題", "本文", null);
        }

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(0, unknown);
        Assert.Equal(0, await service.CountUnreadAsync(Admin, TestContext.Current.CancellationToken));
        var notification = Assert.Single(await service.QueryLatestAsync(User, 10, TestContext.Current.CancellationToken));
        Assert.Equal("data", notification.Link);
        Assert.Equal(User, Assert.Single(notified));
    }

    [Fact]
    public async Task MarkReadChangesOnlyOwnNotification()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        await AddAccountAsync(services, User);
        var service = services.GetRequiredService<NotificationService>();
        using (Begin(services, Admin))
        {
            await service.SendAsync(null, "題", "本文", null);
        }

        var notification = Assert.Single(await service.QueryLatestAsync(User, 10, TestContext.Current.CancellationToken));

        // Act
        bool byAdmin;
        bool byUser;
        using (Begin(services, Admin))
        {
            byAdmin = await service.MarkReadAsync(notification.Id);
        }

        using (Begin(services, User))
        {
            byUser = await service.MarkReadAsync(notification.Id);
        }

        // Assert
        Assert.False(byAdmin);
        Assert.True(byUser);
        Assert.Equal(0, await service.CountUnreadAsync(User, TestContext.Current.CancellationToken));
        Assert.Equal(1, await service.CountUnreadAsync(Admin, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MarkAllReadClearsUnreadPage()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        var service = services.GetRequiredService<NotificationService>();
        using (Begin(services, Admin))
        {
            for (var i = 1; i <= 3; i++)
            {
                await service.SendAsync(Admin, $"題 {i}", "本文", null);
            }
        }

        string[] expected = ["題 3", "題 2"];

        // Act
        var before = await service.QueryPageAsync(Admin, true, 0, 2, TestContext.Current.CancellationToken);
        int count;
        using (Begin(services, Admin))
        {
            count = await service.MarkAllReadAsync();
        }

        var after = await service.QueryPageAsync(Admin, true, 0, 2, TestContext.Current.CancellationToken);
        var all = await service.QueryPageAsync(Admin, false, 0, 10, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, before.Total);
        Assert.Equal(expected, before.Items.Select(static x => x.Title));
        Assert.Equal(3, count);
        Assert.Equal(0, after.Total);
        Assert.Equal(3, all.Total);
        Assert.All(all.Items, static x => Assert.NotNull(x.ReadAt));
    }

    [Fact]
    public async Task RetentionWorkerDeletesOldNotifications()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        var service = services.GetRequiredService<NotificationService>();
        using (Begin(services, Admin, DateTimeOffset.Now.AddDays(-40)))
        {
            for (var i = 1; i <= 5; i++)
            {
                await service.SendAsync(Admin, $"古い {i}", "本文", null);
            }
        }

        using (Begin(services, Admin))
        {
            await service.SendAsync(Admin, "新しい", "本文", null);
        }

        var option = new NotificationRetentionWorkerOption { Enable = true, Days = 30, BatchSize = 2 };
        using var worker = new NotificationRetentionWorker(NullLogger<NotificationRetentionWorker>.Instance, TimeProvider.System, option, service);

        // Act
        await worker.StartAsync(TestContext.Current.CancellationToken);
        var limit = DateTime.UtcNow.AddSeconds(10);
        while ((await service.CountUnreadAsync(Admin, TestContext.Current.CancellationToken) > 1) && (DateTime.UtcNow < limit))
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        await worker.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        var notification = Assert.Single(await service.QueryLatestAsync(Admin, 10, TestContext.Current.CancellationToken));
        Assert.Equal("新しい", notification.Title);
    }

    [Fact]
    public async Task DeletingAccountDeletesNotifications()
    {
        // Arrange
        await using var factory = new TestApplicationFactory();
        var services = factory.Services;
        await AddAccountAsync(services, User);
        var service = services.GetRequiredService<NotificationService>();
        using (Begin(services, Admin))
        {
            await service.SendAsync(User, "題", "本文", null);
        }

        // Act
        await using (var con = services.GetRequiredService<IDbProvider>().CreateConnection())
        {
            await con.OpenAsync(TestContext.Current.CancellationToken);
            await using var command = con.CreateCommand();
            command.CommandText = "DELETE FROM Account WHERE Name = 'user'";
            await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Empty(await service.QueryLatestAsync(User, 10, TestContext.Current.CancellationToken));
    }

    private static IDisposable Begin(IServiceProvider services, string userName) =>
        Begin(services, userName, DateTimeOffset.Now);

    private static IDisposable Begin(IServiceProvider services, string userName, DateTimeOffset now) =>
        services.GetRequiredService<ApplicationServiceContextProvider>().Begin(() => new ServiceContext(now, userName));

    private static async Task AddAccountAsync(IServiceProvider services, string name)
    {
        var password = services.GetRequiredService<IPasswordProvider>().Generate("password");
        await services.GetRequiredService<AccountAccessor>().InsertAsync(name, password, "User", DateTime.Now);
    }
}
