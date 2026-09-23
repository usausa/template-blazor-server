namespace Template.BlazorServer.Services;

using Template.BlazorServer.Accessors;
using Template.BlazorServer.Models;
using Template.BlazorServer.Models.Entity;

public sealed class DataService
{
    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    private readonly ServiceContextProvider contextProvider;

    public DataService(
        IDialect dialect,
        DataAccessor dataAccessor,
        ServiceContextProvider contextProvider)
    {
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
        this.contextProvider = contextProvider;
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
            entity.Id = await dataAccessor.InsertAsync(entity.Name, entity.Value, entity.CreatedAt);
            return DataWriteStatus.Success;
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }
    }

    public async ValueTask<DataWriteStatus> UpdateAsync(long id, string name, int value)
    {
        try
        {
            var rows = await dataAccessor.UpdateAsync(id, name, value);
            return rows > 0 ? DataWriteStatus.Success : DataWriteStatus.NotFound;
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }
    }

    public async ValueTask<DataWriteStatus> DeleteAsync(long id) =>
        await dataAccessor.DeleteAsync(id) > 0 ? DataWriteStatus.Success : DataWriteStatus.NotFound;
}
