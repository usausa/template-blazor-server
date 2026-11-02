namespace Template.BlazorServer.Accessors;

[DataAccessor]
[ExecuteConfig(typeof(DataProfile))]
public sealed partial class NotificationAccessor
{
    [ExecuteScalar]
    public partial ValueTask<int> CountUnreadAsync(string userName, CancellationToken cancellationToken);

    [Query]
    public partial ValueTask<List<NotificationEntity>> QueryLatestAsync(string userName, int limit, CancellationToken cancellationToken);

    [ExecuteScalar]
    public partial ValueTask<int> CountAsync(string userName, bool unreadOnly, CancellationToken cancellationToken);

    [Query]
    public partial ValueTask<List<NotificationEntity>> QueryPageAsync(string userName, bool unreadOnly, int limit, int offset, CancellationToken cancellationToken);

    [Execute]
    public partial ValueTask<int> InsertAsync(string? userName, string title, string body, string? link, DateTime createdAt);

    [Execute]
    public partial ValueTask<int> UpdateReadAsync(long id, string userName, DateTime readAt);

    [Execute]
    public partial ValueTask<int> UpdateReadAllAsync(string userName, DateTime readAt);

    [Execute]
    public partial ValueTask<int> DeleteBeforeAsync(DateTime before, int limit);
}
