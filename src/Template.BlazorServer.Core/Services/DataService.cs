namespace Template.BlazorServer.Services;

using Template.BlazorServer.Accessors;
using Template.BlazorServer.Models;
using Template.BlazorServer.Models.Entity;

public sealed class DataService
{
    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    private readonly ServiceContextProvider contextProvider;

    private readonly DataChangeNotifier changeNotifier;

    public DataService(
        IDialect dialect,
        DataAccessor dataAccessor,
        ServiceContextProvider contextProvider,
        DataChangeNotifier changeNotifier)
    {
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
        this.contextProvider = contextProvider;
        this.changeNotifier = changeNotifier;
    }

    public ValueTask<int> CountAsync(string? name, CancellationToken cancellationToken = default) =>
        dataAccessor.CountAsync(dialect.Match(name), cancellationToken);

    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, DataSort sort, bool desc, int page, int size, CancellationToken cancellationToken = default)
    {
        var pattern = dialect.Match(name);
        var total = await dataAccessor.CountAsync(pattern, cancellationToken);
        var items = await dataAccessor.QueryPageAsync(pattern, sort, desc, size, page * size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    public ValueTask<List<DataEntity>> QueryAllAsync(CancellationToken cancellationToken = default) =>
        dataAccessor.QueryAllAsync(cancellationToken);

    public IAsyncEnumerable<DataEntity> QueryExportEnumerable(CancellationToken cancellationToken) =>
        dataAccessor.QueryExportEnumerable(cancellationToken);

    public ValueTask<DataEntity?> QueryAsync(long id) =>
        dataAccessor.QueryAsync(id);

    public async ValueTask<DataWriteStatus> InsertAsync(DataEntity entity)
    {
        var context = contextProvider.Current;

        try
        {
            entity.CreatedAt = context.Now.DateTime;
            entity.Version = 1;
            entity.Id = await dataAccessor.InsertAsync(entity.Name, entity.Value, entity.CreatedAt);
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }

        changeNotifier.Notify();
        return DataWriteStatus.Success;
    }

    public async ValueTask<DataUpdateResult> UpdateAsync(long id, string name, int value, int? version = null)
    {
        int? updated;
        try
        {
            updated = await dataAccessor.UpdateAsync(id, name, value, version);
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return new DataUpdateResult(DataWriteStatus.Duplicate, 0);
        }

        if (updated.HasValue)
        {
            changeNotifier.Notify();
            return new DataUpdateResult(DataWriteStatus.Success, updated.Value);
        }

        var exists = version.HasValue && await dataAccessor.QueryAsync(id) is not null;
        return new DataUpdateResult(exists ? DataWriteStatus.VersionMismatch : DataWriteStatus.NotFound, 0);
    }

    public async ValueTask<DataWriteStatus> DeleteAsync(long id)
    {
        if (await dataAccessor.DeleteAsync(id) == 0)
        {
            return DataWriteStatus.NotFound;
        }

        changeNotifier.Notify();
        return DataWriteStatus.Success;
    }
}
