namespace Template.BlazorServer.Accessors;

[DataAccessor]
[ExecuteConfig(typeof(DataProfile))]
public sealed partial class AccountAccessor
{
    [ExecuteScalar]
    public partial ValueTask<int> CountAsync();

    [Query]
    public partial ValueTask<List<AccountEntity>> QueryAllAsync(CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<AccountEntity?> QueryByNameAsync(string name);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, byte[] password, string role, DateTime createdAt);
}
