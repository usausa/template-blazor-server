namespace Template.BlazorServer.Services;

using Template.BlazorServer.Accessors;

public sealed class NotificationService
{
    private readonly NotificationAccessor notificationAccessor;

    private readonly ServiceContextProvider contextProvider;

    private readonly NotificationChangeNotifier changeNotifier;

    public NotificationService(
        NotificationAccessor notificationAccessor,
        ServiceContextProvider contextProvider,
        NotificationChangeNotifier changeNotifier)
    {
        this.notificationAccessor = notificationAccessor;
        this.contextProvider = contextProvider;
        this.changeNotifier = changeNotifier;
    }

    public ValueTask<int> CountUnreadAsync(string userName, CancellationToken cancellationToken = default) =>
        notificationAccessor.CountUnreadAsync(userName, cancellationToken);

    public ValueTask<List<NotificationEntity>> QueryLatestAsync(string userName, int limit, CancellationToken cancellationToken = default) =>
        notificationAccessor.QueryLatestAsync(userName, limit, cancellationToken);

    public async ValueTask<PagedResult<NotificationEntity>> QueryPageAsync(string userName, bool unreadOnly, int page, int size, CancellationToken cancellationToken = default)
    {
        var total = await notificationAccessor.CountAsync(userName, unreadOnly, cancellationToken);
        var items = await notificationAccessor.QueryPageAsync(userName, unreadOnly, size, page * size, cancellationToken);
        return new PagedResult<NotificationEntity>(total, page, size, items);
    }

    public async ValueTask<int> SendAsync(string? userName, string title, string body, string? link)
    {
        var context = contextProvider.Current;

        var count = await notificationAccessor.InsertAsync(userName, title, body, link, context.Now.UtcDateTime);
        if (count > 0)
        {
            changeNotifier.Notify(userName);
        }

        return count;
    }

    public async ValueTask<bool> MarkReadAsync(long id)
    {
        var context = contextProvider.Current;

        if (await notificationAccessor.UpdateReadAsync(id, context.UserId, context.Now.UtcDateTime) == 0)
        {
            return false;
        }

        changeNotifier.Notify(context.UserId);
        return true;
    }

    public async ValueTask<int> MarkAllReadAsync()
    {
        var context = contextProvider.Current;

        var count = await notificationAccessor.UpdateReadAllAsync(context.UserId, context.Now.UtcDateTime);
        if (count > 0)
        {
            changeNotifier.Notify(context.UserId);
        }

        return count;
    }

    public async ValueTask<int> DeleteBeforeAsync(DateTime before, int limit)
    {
        var count = await notificationAccessor.DeleteBeforeAsync(before, limit);
        if (count > 0)
        {
            changeNotifier.Notify(null);
        }

        return count;
    }
}
