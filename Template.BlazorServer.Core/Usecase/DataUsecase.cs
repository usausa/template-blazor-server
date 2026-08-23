namespace Template.BlazorServer.Usecase;

using Template.BlazorServer.Models;
using Template.BlazorServer.Models.Entity;
using Template.BlazorServer.Services;

public sealed class DataUsecase
{
    private readonly DataService dataService;

    public DataUsecase(DataService dataService)
    {
        this.dataService = dataService;
    }

    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, int page, int size)
    {
        var total = await dataService.CountAsync(name);
        var items = await dataService.QueryPageAsync(name, page * size, size);
        return new PagedResult<DataEntity>(total, page, size, items);
    }
}
