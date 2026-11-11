namespace Template.BlazorServer.Accessors;

[DataAccessor]
[ExecuteConfig(typeof(DataProfile))]
public sealed partial class CalendarEventAccessor
{
    [Query]
    public partial ValueTask<List<CalendarEventEntity>> QueryRangeAsync(DateTime startAt, DateTime endAt, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<CalendarEventEntity?> QueryAsync(long id);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(long calendarId, string title, DateTime startAt, DateTime endAt, bool allDay, string? description, DateTime createdAt);

    [ExecuteScalar]
    public partial ValueTask<int?> UpdateAsync(long id, long calendarId, string title, DateTime startAt, DateTime endAt, bool allDay, string? description, int? version);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
