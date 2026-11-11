namespace Template.BlazorServer.Host.Endpoints;

using Smart.Mapper;

using Template.BlazorServer.Host.Infrastructure.Http;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class CalendarListEntry
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public string Color { get; set; } = default!;

    public int Version { get; set; }
}

public sealed class CalendarListResponse
{
    public IReadOnlyList<CalendarListEntry> Items { get; set; } = default!;
}

public sealed class CalendarEventListEntry
{
    public long Id { get; set; }

    public long CalendarId { get; set; }

    public string Title { get; set; } = default!;

    public bool AllDay { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public int Version { get; set; }
}

public sealed class CalendarEventListResponse
{
    public IReadOnlyList<CalendarEventListEntry> Items { get; set; } = default!;
}

public sealed class CalendarEventGetResponse
{
    public long Id { get; set; }

    public long CalendarId { get; set; }

    public string Title { get; set; } = default!;

    public bool AllDay { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public int Version { get; set; }
}

public sealed class CalendarEventCreateRequest
{
    public long CalendarId { get; set; }

    [Required]
    [MaxLength(Length.CalendarEventTitle)]
    public string Title { get; set; } = default!;

    public bool AllDay { get; set; }

    public DateTimeOffset? StartAt { get; set; }

    public DateTimeOffset? EndAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [MaxLength(Length.CalendarEventDescription)]
    public string? Description { get; set; }
}

public sealed class CalendarEventCreateResponse
{
    public long Id { get; set; }
}

public sealed class CalendarEventUpdateRequest
{
    public long CalendarId { get; set; }

    [Required]
    [MaxLength(Length.CalendarEventTitle)]
    public string Title { get; set; } = default!;

    public bool AllDay { get; set; }

    public DateTimeOffset? StartAt { get; set; }

    public DateTimeOffset? EndAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [MaxLength(Length.CalendarEventDescription)]
    public string? Description { get; set; }
}

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class CalendarMapper
{
    [Mapper]
    public static partial CalendarListEntry ToListEntry(this CalendarEntity entity);

    public static CalendarEventListEntry ToListEntry(this CalendarEventEntity entity) =>
        new()
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Title = entity.Title,
            AllDay = entity.AllDay,
            StartAt = entity.AllDay ? null : entity.StartAt,
            EndAt = entity.AllDay ? null : entity.EndAt,
            StartDate = entity.AllDay ? DateOnly.FromDateTime(entity.StartAt) : null,
            EndDate = entity.AllDay ? DateOnly.FromDateTime(entity.EndAt) : null,
            Description = entity.Description,
            Version = entity.Version
        };

    public static CalendarEventGetResponse ToGetResponse(this CalendarEventEntity entity) =>
        new()
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Title = entity.Title,
            AllDay = entity.AllDay,
            StartAt = entity.AllDay ? null : entity.StartAt,
            EndAt = entity.AllDay ? null : entity.EndAt,
            StartDate = entity.AllDay ? DateOnly.FromDateTime(entity.StartAt) : null,
            EndDate = entity.AllDay ? DateOnly.FromDateTime(entity.EndAt) : null,
            Description = entity.Description,
            Version = entity.Version
        };
}

//--------------------------------------------------------------------------------
// Endpoints
//--------------------------------------------------------------------------------

public static class CalendarEndpoints
{
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(366);

    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapCalendarEndpoints(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Calendar)
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/calendars", HandleCalendarListAsync)
            .WithName("CalendarList")
            .Produces<CalendarListResponse>();
        group.MapGet("/events", HandleEventListAsync)
            .WithName("CalendarEventList")
            .Produces<CalendarEventListResponse>()
            .ProducesValidationProblem();
        group.MapGet("/events/{id:long}", HandleEventGetAsync)
            .WithName("CalendarEventGet")
            .Produces<CalendarEventGetResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/events", HandleEventCreateAsync)
            .WithName("CalendarEventCreate")
            .Produces<CalendarEventCreateResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        group.MapPut("/events/{id:long}", HandleEventUpdateAsync)
            .WithName("CalendarEventUpdate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed);
        group.MapDelete("/events/{id:long}", HandleEventDeleteAsync)
            .WithName("CalendarEventDelete")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    //--------------------------------------------------------------------------------
    // Calendar list
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleCalendarListAsync(
        CalendarService calendarService,
        CancellationToken cancellationToken)
    {
        var calendars = await calendarService.QueryCalendarListAsync(cancellationToken);
        return TypedResults.Ok(new CalendarListResponse
        {
            Items = calendars.Select(static x => x.ToListEntry()).ToList()
        });
    }

    //--------------------------------------------------------------------------------
    // Event list
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleEventListAsync(
        CalendarService calendarService,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        if ((end <= start) || (end - start > MaxRange))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["end"] = ["end must be after start and within 366 days."]
            });
        }

        var startDate = DateOnly.FromDateTime(start.DateTime);
        var endDate = DateOnly.FromDateTime(end.DateTime);
        if (end.TimeOfDay > TimeSpan.Zero)
        {
            endDate = endDate.AddDays(1);
        }

        var events = await calendarService.QueryEventListAsync(start.UtcDateTime, end.UtcDateTime, startDate, endDate, cancellationToken);
        return TypedResults.Ok(new CalendarEventListResponse
        {
            Items = events.Select(static x => x.ToListEntry()).ToList()
        });
    }

    //--------------------------------------------------------------------------------
    // Event get
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleEventGetAsync(
        CalendarService calendarService,
        HttpResponse response,
        long id)
    {
        var entity = await calendarService.QueryEventAsync(id);
        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        response.Headers.ETag = EntityTag.From(entity.Version);
        return TypedResults.Ok(entity.ToGetResponse());
    }

    //--------------------------------------------------------------------------------
    // Event create
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleEventCreateAsync(
        CalendarService calendarService,
        CalendarEventCreateRequest request)
    {
        if (!TryReadTimes(request.AllDay, request.StartAt, request.EndAt, request.StartDate, request.EndDate, out var start, out var end))
        {
            return TimeProblem(request.AllDay);
        }

        var entity = new CalendarEventEntity
        {
            CalendarId = request.CalendarId,
            Title = request.Title,
            StartAt = start,
            EndAt = end,
            AllDay = request.AllDay,
            Description = request.Description
        };
        return await calendarService.InsertEventAsync(entity) == CalendarWriteStatus.Success
            ? TypedResults.Created($"{ApiRoutes.Calendar}/events/{entity.Id}", new CalendarEventCreateResponse { Id = entity.Id })
            : CalendarProblem();
    }

    //--------------------------------------------------------------------------------
    // Event update
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleEventUpdateAsync(
        CalendarService calendarService,
        HttpResponse response,
        long id,
        CalendarEventUpdateRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch)
    {
        if (!EntityTag.TryParseIfMatch(ifMatch, out var version))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status412PreconditionFailed, title: "Version mismatch.");
        }

        if (!TryReadTimes(request.AllDay, request.StartAt, request.EndAt, request.StartDate, request.EndDate, out var start, out var end))
        {
            return TimeProblem(request.AllDay);
        }

        var entity = new CalendarEventEntity
        {
            Id = id,
            CalendarId = request.CalendarId,
            Title = request.Title,
            StartAt = start,
            EndAt = end,
            AllDay = request.AllDay,
            Description = request.Description
        };
        var result = await calendarService.UpdateEventAsync(entity, version);
        switch (result.Status)
        {
            case CalendarWriteStatus.Success:
                response.Headers.ETag = EntityTag.From(result.Version);
                return TypedResults.NoContent();
            case CalendarWriteStatus.VersionMismatch:
                return TypedResults.Problem(statusCode: StatusCodes.Status412PreconditionFailed, title: "Version mismatch.");
            case CalendarWriteStatus.CalendarNotFound:
                return CalendarProblem();
            default:
                return TypedResults.NotFound();
        }
    }

    //--------------------------------------------------------------------------------
    // Event delete
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleEventDeleteAsync(
        CalendarService calendarService,
        long id)
    {
        var result = await calendarService.DeleteEventAsync(id);
        return result == CalendarWriteStatus.Success ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static bool TryReadTimes(bool allDay, DateTimeOffset? startAt, DateTimeOffset? endAt, DateOnly? startDate, DateOnly? endDate, out DateTime start, out DateTime end)
    {
        if (allDay)
        {
            start = startDate?.ToDateTime(TimeOnly.MinValue) ?? default;
            end = endDate?.ToDateTime(TimeOnly.MinValue) ?? default;
            return endDate > startDate;
        }

        start = startAt?.UtcDateTime ?? default;
        end = endAt?.UtcDateTime ?? default;
        return endAt > startAt;
    }

    private static ValidationProblem TimeProblem(bool allDay) =>
        allDay
            ? TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["endDate"] = ["startDate and endDate are required, and endDate must be after startDate."]
            })
            : TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["endAt"] = ["startAt and endAt are required, and endAt must be after startAt."]
            });

    private static ValidationProblem CalendarProblem() =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["calendarId"] = ["The calendar does not exist."]
        });
}
