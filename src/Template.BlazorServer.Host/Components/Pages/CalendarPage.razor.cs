namespace Template.BlazorServer.Host.Components.Pages;

using BlazorComponents;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using MudBlazor;

using MusselCalendar;

using Template.BlazorServer.Host.Components.Dialogs;
using Template.BlazorServer.Host.Components.Shared;

public sealed partial class CalendarPage : IAsyncDisposable
{
    private const string ModulePath = "./js/calendar.js";

    private const string ViewKey = "calendar.view";

    private const string HiddenKey = "calendar.hidden";

    private const string Untitled = "(タイトルなし)";

    private const int DayCount = 4;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

    private readonly Dictionary<string, CalendarEventEntity> events = new(StringComparer.Ordinal);

    private readonly HashSet<long> hidden = [];

    private ScriptModule? module;

    private RefreshTimer? refreshTimer;

    private TimeZoneInfo zone = TimeZoneInfo.Local;

    private ZoneTimeProvider clock = default!;

    private List<CalendarEntity> calendars = [];

    private List<CalendarItem> items = [];

    private CalendarViewKind view = CalendarViewKind.Week;

    private DateOnly date;

    private bool ready;

    private bool dialogOpen;

    private CalendarSelectEventArgs? draft;

    private string draftTitle = string.Empty;

    private long draftCalendarId;

    private CalendarItemEventArgs? selected;

    private CalendarMoreEventArgs? more;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required TimeProvider TimeProvider { get; set; }

    [Inject]
    public required RefreshTimerFactory RefreshTimerFactory { get; set; }

    [Inject]
    public required CalendarService CalendarService { get; set; }

    [Inject]
    public required CalendarChangeNotifier CalendarChangeNotifier { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private static DayOfWeek FirstDayOfWeek => CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

    private CalendarRange Range => CalendarRange.Of(view, date, FirstDayOfWeek, DayCount);

    private bool KeysDisabled => !ready || dialogOpen;

    private bool HasPopup => (draft is not null) || (selected is not null) || (more is not null);

    private CalendarEventEntity? SelectedEntity =>
        (selected is not null) && events.TryGetValue(selected.Item.Id, out var entity) ? entity : null;

    private string Title
    {
        get
        {
            var culture = CultureInfo.CurrentCulture;
            if (view == CalendarViewKind.Day)
            {
                return date.ToString("yyyy年M月d日 (ddd)", culture);
            }

            if (view == CalendarViewKind.Month)
            {
                return date.ToString("yyyy年M月", culture);
            }

            var range = Range;
            var first = range.Start;
            var last = range.End.AddDays(-1);
            if (first.Month == last.Month)
            {
                return first.ToString("yyyy年M月", culture);
            }

            return $"{first.ToString("yyyy年M月", culture)} – {last.ToString(first.Year == last.Year ? "M月" : "yyyy年M月", culture)}";
        }
    }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        clock = new ZoneTimeProvider(TimeProvider, zone);
        date = Today();
        refreshTimer = RefreshTimerFactory.Create(RefreshInterval, RefreshAsync);
        CalendarChangeNotifier.Changed += OnCalendarChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        module = new ScriptModule(JSRuntime, ModulePath);
        zone = CalendarTime.FindZone(await module.InvokeAsync<string?>("timeZone"));
        clock = new ZoneTimeProvider(TimeProvider, zone);
        date = Today();
        view = RequestHelper.Parse(await module.InvokeAsync<string?>("load", ViewKey), CalendarViewKind.Week);
        foreach (var value in (await module.InvokeAsync<string?>("load", HiddenKey) ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Int64.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                hidden.Add(id);
            }
        }

        await LoadAsync();
        ready = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.DisposeAsync();
        }

        Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CalendarChangeNotifier.Changed -= OnCalendarChanged;
            refreshTimer?.Dispose();
            refreshTimer = null;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnCalendarChanged(object? sender, EventArgs e) => refreshTimer?.Request();

    private Task RefreshAsync() =>
        InvokeAsync(async () =>
        {
            if (!ready)
            {
                return;
            }

            await LoadAsync();
            StateHasChanged();
        });

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    private async Task ChangeViewAsync(CalendarViewKind value)
    {
        Close();
        view = value;
        await SaveAsync(ViewKey, view.ToString());
        await LoadAsync();
    }

    private Task GoTodayAsync()
    {
        Close();
        date = Today();
        return LoadAsync();
    }

    private Task MoveAsync(int direction)
    {
        Close();
        date = CalendarRange.Move(view, date, direction, DayCount);
        return LoadAsync();
    }

    private Task ChangeDateAsync(DateOnly value)
    {
        Close();
        date = value;
        return LoadAsync();
    }

    private Task OnDayClickedAsync(DateOnly day)
    {
        date = day;
        return ChangeViewAsync(CalendarViewKind.Day);
    }

    //--------------------------------------------------------------------------------
    // Calendar
    //--------------------------------------------------------------------------------

    private bool IsVisible(CalendarEntity calendar) => !hidden.Contains(calendar.Id);

    private async Task SetVisibleAsync(CalendarEntity calendar, bool visible)
    {
        if (visible)
        {
            hidden.Remove(calendar.Id);
        }
        else
        {
            hidden.Add(calendar.Id);
        }

        await SaveHiddenAsync();
        UpdateItems();
    }

    private async Task AddCalendarAsync()
    {
        var entity = await ShowDialogAsync(() => DialogService.ShowCalendarEditDialog("カレンダーの追加", null));
        if (entity is null)
        {
            return;
        }

        var status = await CalendarService.InsertCalendarAsync(entity);
        if (status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("追加しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(status));
        }

        await LoadAsync();
    }

    private async Task EditCalendarAsync(CalendarEntity calendar)
    {
        var edited = await ShowDialogAsync(() => DialogService.ShowCalendarEditDialog("カレンダーの編集", calendar));
        if (edited is null)
        {
            return;
        }

        var result = await CalendarService.UpdateCalendarAsync(calendar.Id, edited.Name, edited.Color, calendar.Version);
        if (result.Status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("更新しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(result.Status));
        }

        await LoadAsync();
    }

    private async Task DeleteCalendarAsync(CalendarEntity calendar)
    {
        if (!await ShowDialogAsync(() => DialogService.ShowConfirm("カレンダーの削除", $"{calendar.Name} と予定を削除してよろしいですか？")))
        {
            return;
        }

        var status = await CalendarService.DeleteCalendarAsync(calendar.Id);
        if (status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("削除しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(status));
        }

        if (hidden.Remove(calendar.Id))
        {
            await SaveHiddenAsync();
        }

        await LoadAsync();
    }

    //--------------------------------------------------------------------------------
    // Event of calendar
    //--------------------------------------------------------------------------------

    private void OnRangeSelected(CalendarSelectEventArgs e)
    {
        Close();
        draft = e;
        draftTitle = string.Empty;
        draftCalendarId = DefaultCalendarId();
    }

    private Task OnDraftKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            return SaveDraftAsync();
        }

        if (e.Key == "Escape")
        {
            Close();
        }

        return Task.CompletedTask;
    }

    private Task SaveDraftAsync()
    {
        if (draft is null)
        {
            return Task.CompletedTask;
        }

        var title = String.IsNullOrWhiteSpace(draftTitle) ? Untitled : draftTitle.Trim();
        var input = new CalendarEventInput(title, draftCalendarId, draft.Start, draft.End, draft.AllDay, null);
        Close();
        return InsertAsync(input);
    }

    private Task MoreOptionsAsync()
    {
        if (draft is null)
        {
            return Task.CompletedTask;
        }

        var input = new CalendarEventInput(draftTitle.Trim(), draftCalendarId, draft.Start, draft.End, draft.AllDay, null);
        Close();
        return CreateAsync(input);
    }

    private Task CreateAsync()
    {
        Close();
        var now = clock.GetLocalNow().DateTime;
        var start = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Unspecified).AddHours(1);
        return CreateAsync(new CalendarEventInput(string.Empty, DefaultCalendarId(), start, start.AddHours(1), false, null));
    }

    private async Task CreateAsync(CalendarEventInput input)
    {
        var edited = await ShowDialogAsync(() => DialogService.ShowCalendarEventDialog("予定の作成", input, calendars));
        if (edited is not null)
        {
            await InsertAsync(edited);
        }
    }

    private async Task InsertAsync(CalendarEventInput input)
    {
        var status = await CalendarService.InsertEventAsync(ToEntity(input, 0));
        if (status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("予定を作成しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(status));
        }

        await LoadAsync();
    }

    private void OnItemClicked(CalendarItemEventArgs e)
    {
        Close();
        selected = e;
    }

    private async Task EditSelectedAsync()
    {
        if ((selected is null) || (SelectedEntity is not { } entity))
        {
            return;
        }

        var item = selected.Item;
        Close();
        var input = new CalendarEventInput(entity.Title, entity.CalendarId, item.Start, item.End, item.AllDay, entity.Description);
        var edited = await ShowDialogAsync(() => DialogService.ShowCalendarEventDialog("予定の編集", input, calendars));
        if (edited is null)
        {
            return;
        }

        var result = await CalendarService.UpdateEventAsync(ToEntity(edited, entity.Id), entity.Version);
        if (result.Status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("更新しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(result.Status));
        }

        await LoadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        if (SelectedEntity is not { } entity)
        {
            return;
        }

        Close();
        if (!await ShowDialogAsync(() => DialogService.ShowConfirm("予定の削除", $"{entity.Title} を削除してよろしいですか？")))
        {
            return;
        }

        var status = await CalendarService.DeleteEventAsync(entity.Id);
        if (status == CalendarWriteStatus.Success)
        {
            Snackbar.AddSuccess("削除しました。");
        }
        else
        {
            Snackbar.AddError(ErrorMessage(status));
        }

        await LoadAsync();
    }

    private async Task OnItemChangedAsync(CalendarItemChangeEventArgs e)
    {
        if (!events.TryGetValue(e.Item.Id, out var entity))
        {
            return;
        }

        var changed = new CalendarEventEntity
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Title = entity.Title,
            StartAt = CalendarTime.ToStore(e.Start, e.AllDay, zone),
            EndAt = CalendarTime.ToStore(e.End, e.AllDay, zone),
            AllDay = e.AllDay,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
        var result = await CalendarService.UpdateEventAsync(changed, entity.Version);
        if (result.Status == CalendarWriteStatus.Success)
        {
            changed.Version = result.Version;
            events[e.Item.Id] = changed;
            e.Item.Start = e.Start;
            e.Item.End = e.End;
            e.Item.AllDay = e.AllDay;
            return;
        }

        Snackbar.AddError(ErrorMessage(result.Status));
        await LoadAsync();
    }

    private void OnMoreClicked(CalendarMoreEventArgs e)
    {
        Close();
        more = e;
    }

    private void OpenFromMore(CalendarItem item)
    {
        var anchor = more?.Anchor ?? default;
        Close();
        selected = new CalendarItemEventArgs(item, anchor);
    }

    private void Close()
    {
        draft = null;
        selected = null;
        more = null;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private DateOnly Today() => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    private long DefaultCalendarId() =>
        (calendars.FirstOrDefault(x => !hidden.Contains(x.Id)) ?? calendars.FirstOrDefault())?.Id ?? 0;

    private async Task LoadAsync()
    {
        calendars = await CalendarService.QueryCalendarListAsync();
        var range = Range;
        var list = await CalendarService.QueryEventListAsync(CalendarTime.ToUtc(range.StartTime, zone), CalendarTime.ToUtc(range.EndTime, zone), range.Start, range.End);
        events.Clear();
        foreach (var entity in list)
        {
            events[entity.Id.ToString(CultureInfo.InvariantCulture)] = entity;
        }

        UpdateItems();
    }

    private void UpdateItems()
    {
        var colors = calendars.ToDictionary(static x => x.Id, static x => x.Color);
        items = events.Values
            .Where(x => colors.ContainsKey(x.CalendarId) && !hidden.Contains(x.CalendarId))
            .Select(x => ToItem(x, colors[x.CalendarId]))
            .ToList();
    }

    private CalendarItem ToItem(CalendarEventEntity entity, string color) =>
        new()
        {
            Id = entity.Id.ToString(CultureInfo.InvariantCulture),
            Title = entity.Title,
            Start = CalendarTime.ToLocal(entity.StartAt, entity.AllDay, zone),
            End = CalendarTime.ToLocal(entity.EndAt, entity.AllDay, zone),
            AllDay = entity.AllDay,
            Color = color
        };

    private CalendarEventEntity ToEntity(CalendarEventInput input, long id) =>
        new()
        {
            Id = id,
            CalendarId = input.CalendarId,
            Title = input.Title,
            StartAt = CalendarTime.ToStore(input.Start, input.AllDay, zone),
            EndAt = CalendarTime.ToStore(input.End, input.AllDay, zone),
            AllDay = input.AllDay,
            Description = input.Description
        };

    private async Task<T> ShowDialogAsync<T>(Func<ValueTask<T>> show)
    {
        dialogOpen = true;
        try
        {
            return await show();
        }
        finally
        {
            dialogOpen = false;
        }
    }

    private async Task SaveAsync(string key, string value)
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("save", key, value);
        }
    }

    private Task SaveHiddenAsync() =>
        SaveAsync(HiddenKey, String.Join(',', hidden.Order().Select(static x => x.ToString(CultureInfo.InvariantCulture))));

    private string CalendarName(long id) => calendars.FirstOrDefault(x => x.Id == id)?.Name ?? string.Empty;

    private string CalendarColor(long id) => calendars.FirstOrDefault(x => x.Id == id)?.Color ?? CalendarColors.Blue;

    private static string ErrorMessage(CalendarWriteStatus status) =>
        status switch
        {
            CalendarWriteStatus.NotFound => "対象が存在しません。",
            CalendarWriteStatus.Duplicate => "名前が重複しています。",
            CalendarWriteStatus.VersionMismatch => "ほかで更新されています。",
            _ => "カレンダーが存在しません。"
        };

    private static string PopupStyle(CalendarAnchor anchor)
    {
        var left = anchor.Left + anchor.Width + 8;
        return String.Create(CultureInfo.InvariantCulture, $"left: clamp(8px, {left:0}px, calc(100vw - 336px)); top: clamp(8px, {anchor.Top:0}px, calc(100vh - 320px));");
    }

    private static string RangeText(DateTime start, DateTime end, bool allDay)
    {
        var culture = CultureInfo.CurrentCulture;
        if (allDay)
        {
            var last = end.AddDays(-1);
            return last.Date == start.Date
                ? start.ToString("M月d日 (ddd)", culture)
                : $"{start.ToString("M月d日 (ddd)", culture)} – {last.ToString("M月d日 (ddd)", culture)}";
        }

        return (start.Date == end.Date) || (end == start.Date.AddDays(1))
            ? $"{start.ToString("M月d日 (ddd)", culture)} {start.ToString("H:mm", culture)} – {end.ToString("H:mm", culture)}"
            : $"{start.ToString("M月d日 (ddd) H:mm", culture)} – {end.ToString("M月d日 (ddd) H:mm", culture)}";
    }

    //--------------------------------------------------------------------------------
    // View
    //--------------------------------------------------------------------------------

    private sealed class ZoneTimeProvider : TimeProvider
    {
        private readonly TimeProvider inner;

        public ZoneTimeProvider(TimeProvider inner, TimeZoneInfo zone)
        {
            this.inner = inner;
            LocalTimeZone = zone;
        }

        public override TimeZoneInfo LocalTimeZone { get; }

        public override long TimestampFrequency => inner.TimestampFrequency;

        public override DateTimeOffset GetUtcNow() => inner.GetUtcNow();

        public override long GetTimestamp() => inner.GetTimestamp();

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
            inner.CreateTimer(callback, state, dueTime, period);
    }
}
