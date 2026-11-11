namespace Template.BlazorServer.Services;

using Template.BlazorServer.Accessors;
using Template.BlazorServer.Models.Entity;

public sealed class CalendarService
{
    private readonly IDialect dialect;

    private readonly CalendarAccessor calendarAccessor;

    private readonly CalendarEventAccessor eventAccessor;

    private readonly ServiceContextProvider contextProvider;

    private readonly CalendarChangeNotifier changeNotifier;

    public CalendarService(
        IDialect dialect,
        CalendarAccessor calendarAccessor,
        CalendarEventAccessor eventAccessor,
        ServiceContextProvider contextProvider,
        CalendarChangeNotifier changeNotifier)
    {
        this.dialect = dialect;
        this.calendarAccessor = calendarAccessor;
        this.eventAccessor = eventAccessor;
        this.contextProvider = contextProvider;
        this.changeNotifier = changeNotifier;
    }

    public async ValueTask InitializeAsync(IEnumerable<CalendarEntity> calendars)
    {
        if (await calendarAccessor.CountAsync() > 0)
        {
            return;
        }

        var context = contextProvider.Current;
        foreach (var calendar in calendars)
        {
            await calendarAccessor.InsertAsync(calendar.Name, calendar.Color, context.Now.UtcDateTime);
        }
    }

    public ValueTask<List<CalendarEntity>> QueryCalendarListAsync(CancellationToken cancellationToken = default) =>
        calendarAccessor.QueryAllAsync(cancellationToken);

    public async ValueTask<CalendarWriteStatus> InsertCalendarAsync(CalendarEntity entity)
    {
        var context = contextProvider.Current;

        try
        {
            entity.CreatedAt = context.Now.UtcDateTime;
            entity.Version = 1;
            entity.Id = await calendarAccessor.InsertAsync(entity.Name, entity.Color, entity.CreatedAt);
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return CalendarWriteStatus.Duplicate;
        }

        changeNotifier.Notify();
        return CalendarWriteStatus.Success;
    }

    public async ValueTask<CalendarUpdateResult> UpdateCalendarAsync(long id, string name, string color, int? version = null)
    {
        int? updated;
        try
        {
            updated = await calendarAccessor.UpdateAsync(id, name, color, version);
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return new CalendarUpdateResult(CalendarWriteStatus.Duplicate, 0);
        }

        if (updated.HasValue)
        {
            changeNotifier.Notify();
            return new CalendarUpdateResult(CalendarWriteStatus.Success, updated.Value);
        }

        var exists = version.HasValue && (await calendarAccessor.QueryAsync(id) is not null);
        return new CalendarUpdateResult(exists ? CalendarWriteStatus.VersionMismatch : CalendarWriteStatus.NotFound, 0);
    }

    public async ValueTask<CalendarWriteStatus> DeleteCalendarAsync(long id)
    {
        if (await calendarAccessor.DeleteAsync(id) == 0)
        {
            return CalendarWriteStatus.NotFound;
        }

        changeNotifier.Notify();
        return CalendarWriteStatus.Success;
    }

    public ValueTask<List<CalendarEventEntity>> QueryEventListAsync(DateTime startAt, DateTime endAt, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        eventAccessor.QueryRangeAsync(startAt, endAt, startDate.ToDateTime(TimeOnly.MinValue), endDate.ToDateTime(TimeOnly.MinValue), cancellationToken);

    public ValueTask<CalendarEventEntity?> QueryEventAsync(long id) =>
        eventAccessor.QueryAsync(id);

    public async ValueTask<CalendarWriteStatus> InsertEventAsync(CalendarEventEntity entity)
    {
        if (await calendarAccessor.QueryAsync(entity.CalendarId) is null)
        {
            return CalendarWriteStatus.CalendarNotFound;
        }

        var context = contextProvider.Current;
        entity.CreatedAt = context.Now.UtcDateTime;
        entity.Version = 1;
        entity.Id = await eventAccessor.InsertAsync(entity.CalendarId, entity.Title, entity.StartAt, entity.EndAt, entity.AllDay, entity.Description, entity.CreatedAt);

        changeNotifier.Notify();
        return CalendarWriteStatus.Success;
    }

    public async ValueTask<CalendarUpdateResult> UpdateEventAsync(CalendarEventEntity entity, int? version = null)
    {
        if (await calendarAccessor.QueryAsync(entity.CalendarId) is null)
        {
            return new CalendarUpdateResult(CalendarWriteStatus.CalendarNotFound, 0);
        }

        var updated = await eventAccessor.UpdateAsync(entity.Id, entity.CalendarId, entity.Title, entity.StartAt, entity.EndAt, entity.AllDay, entity.Description, version);
        if (updated.HasValue)
        {
            changeNotifier.Notify();
            return new CalendarUpdateResult(CalendarWriteStatus.Success, updated.Value);
        }

        var exists = version.HasValue && (await eventAccessor.QueryAsync(entity.Id) is not null);
        return new CalendarUpdateResult(exists ? CalendarWriteStatus.VersionMismatch : CalendarWriteStatus.NotFound, 0);
    }

    public async ValueTask<CalendarWriteStatus> DeleteEventAsync(long id)
    {
        if (await eventAccessor.DeleteAsync(id) == 0)
        {
            return CalendarWriteStatus.NotFound;
        }

        changeNotifier.Notify();
        return CalendarWriteStatus.Success;
    }
}
