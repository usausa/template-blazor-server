namespace Template.BlazorServer.Accessors;

[DataAccessor]
[ExecuteConfig(typeof(DataProfile))]
public sealed partial class CalendarAccessor
{
    [ExecuteScalar]
    public partial ValueTask<int> CountAsync();

    [Query]
    public partial ValueTask<List<CalendarEntity>> QueryAllAsync(CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<CalendarEntity?> QueryAsync(long id);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, string color, DateTime createdAt);

    [ExecuteScalar]
    public partial ValueTask<int?> UpdateAsync(long id, string name, string color, int? version);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
