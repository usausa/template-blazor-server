namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

using Template.BlazorServer.Host.Components.Dialogs;
using Template.BlazorServer.Host.Components.Shared;

public sealed partial class DataPage
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

#pragma warning disable CA2213
    private MudDataGrid<DataEntity> grid = default!;
#pragma warning restore CA2213

    private RefreshTimer refreshTimer = default!;

    private string? searchName;

    private string? appliedName;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required RefreshTimerFactory RefreshTimerFactory { get; set; }

    [Inject]
    public required DataService DataService { get; set; }

    [Inject]
    public required DataChangeNotifier DataChangeNotifier { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [SupplyParameterFromQuery(Name = "name")]
    public string? Name { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        searchName = Name;
        appliedName = Name;
        refreshTimer = RefreshTimerFactory.Create(RefreshInterval, RefreshAsync);
        DataChangeNotifier.Changed += OnDataChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DataChangeNotifier.Changed -= OnDataChanged;
            refreshTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task OnSearchKeyDown(KeyboardEventArgs args) =>
        args.Key == "Enter" ? SearchAsync() : Task.CompletedTask;

    private void OnDataChanged(object? sender, EventArgs e) => refreshTimer.Request();

    private Task RefreshAsync() => InvokeAsync(grid.ReloadServerData);

    //--------------------------------------------------------------------------------
    // Grid
    //--------------------------------------------------------------------------------

    private async Task<GridData<DataEntity>> LoadServerData(GridState<DataEntity> state, CancellationToken cancellationToken)
    {
        var sort = state.SortDefinitions.FirstOrDefault();
        var result = await DataService.QueryPageAsync(appliedName, RequestHelper.Parse(sort?.SortBy, DataSort.Id), sort?.Descending ?? false, state.Page, state.PageSize, cancellationToken);
        return new GridData<DataEntity>
        {
            TotalItems = result.Total,
            Items = result.Items
        };
    }

    private static string FormatCreatedAt(DataEntity entity) => $"{entity.CreatedAt.ToLocalTime():yyyy/MM/dd HH:mm:ss}";

    private Task SearchAsync()
    {
        appliedName = searchName;

        // Sync search condition to URL
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("name", String.IsNullOrEmpty(searchName) ? null : searchName));
        return grid.ReloadServerData();
    }

    //--------------------------------------------------------------------------------
    // Action
    //--------------------------------------------------------------------------------

    private async Task AddAsync()
    {
        var entity = await DialogService.ShowEditDialog("データ追加", null);
        if (entity is null)
        {
            return;
        }

        if (await DataService.InsertAsync(entity) == DataWriteStatus.Success)
        {
            Snackbar.AddSuccess("追加しました。");
            await grid.ReloadServerData();
        }
        else
        {
            Snackbar.AddError("名前が重複しています。");
        }
    }

    private async Task EditAsync(DataEntity entity)
    {
        var edited = await DialogService.ShowEditDialog("データ編集", entity);
        if (edited is null)
        {
            return;
        }

        var result = await DataService.UpdateAsync(edited.Id, edited.Name, edited.Value, edited.Version);
        switch (result.Status)
        {
            case DataWriteStatus.Success:
                Snackbar.AddSuccess("更新しました。");
                await grid.ReloadServerData();
                break;
            case DataWriteStatus.NotFound:
                Snackbar.AddError("対象が存在しません。");
                await grid.ReloadServerData();
                break;
            case DataWriteStatus.VersionMismatch:
                Snackbar.AddError("ほかで更新されています。");
                await grid.ReloadServerData();
                break;
            default:
                Snackbar.AddError("名前が重複しています。");
                break;
        }
    }

    private async Task DeleteAsync(DataEntity entity)
    {
        if (!await DialogService.ShowConfirm("データ削除", $"{entity.Name} を削除してよろしいですか？"))
        {
            return;
        }

        if (await DataService.DeleteAsync(entity.Id) == DataWriteStatus.Success)
        {
            Snackbar.AddSuccess("削除しました。");
        }
        else
        {
            Snackbar.AddError("対象が存在しません。");
        }

        await grid.ReloadServerData();
    }
}
