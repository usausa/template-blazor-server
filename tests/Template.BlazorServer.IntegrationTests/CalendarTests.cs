namespace Template.BlazorServer;

using System.Globalization;
using System.Net.Http.Json;

using Template.BlazorServer.Host.Endpoints;

public sealed class CalendarTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public CalendarTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task InitialCalendarsAreCreated()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetFromJsonAsync<CalendarListResponse>(new Uri("/api/calendar/calendars", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(["仕事", "個人"], response.Items.Select(static x => x.Name));
        Assert.All(response.Items, static x => Assert.Equal(1, x.Version));
    }

    [Fact]
    public async Task ListReturnsEventsInRange()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        var calendarId = await FirstCalendarIdAsync(client);
        var timed = await CreateAsync(client, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Range timed",
            StartAt = DateTimeOffset.Parse("2030-01-10T10:00:00+09:00", CultureInfo.InvariantCulture),
            EndAt = DateTimeOffset.Parse("2030-01-10T11:00:00+09:00", CultureInfo.InvariantCulture)
        });
        var allDay = await CreateAsync(client, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Range all day",
            AllDay = true,
            StartDate = new DateOnly(2030, 1, 12),
            EndDate = new DateOnly(2030, 1, 14)
        });
        var outside = await CreateAsync(client, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Range outside",
            StartAt = DateTimeOffset.Parse("2030-01-13T00:00:00+09:00", CultureInfo.InvariantCulture),
            EndAt = DateTimeOffset.Parse("2030-01-13T01:00:00+09:00", CultureInfo.InvariantCulture)
        });
        var query = $"/api/calendar/events?start={Uri.EscapeDataString("2030-01-06T00:00:00+09:00")}&end={Uri.EscapeDataString("2030-01-13T00:00:00+09:00")}";

        // Act
        var response = await client.GetFromJsonAsync<CalendarEventListResponse>(new Uri(query, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        var ids = response.Items.Select(static x => x.Id).ToList();
        Assert.Contains(timed, ids);
        Assert.Contains(allDay, ids);
        Assert.DoesNotContain(outside, ids);
        var timedEntry = response.Items.Single(x => x.Id == timed);
        Assert.Equal(new DateTime(2030, 1, 10, 1, 0, 0, DateTimeKind.Utc), timedEntry.StartAt);
        Assert.Equal(DateTimeKind.Utc, timedEntry.StartAt!.Value.Kind);
        Assert.Null(timedEntry.StartDate);
        var allDayEntry = response.Items.Single(x => x.Id == allDay);
        Assert.True(allDayEntry.AllDay);
        Assert.Equal(new DateOnly(2030, 1, 12), allDayEntry.StartDate);
        Assert.Equal(new DateOnly(2030, 1, 14), allDayEntry.EndDate);
        Assert.Null(allDayEntry.StartAt);
    }

    [Fact]
    public async Task UpdateRejectsStaleVersion()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        var calendarId = await FirstCalendarIdAsync(client);
        var id = await CreateAsync(client, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Version",
            StartAt = DateTimeOffset.Parse("2030-02-01T09:00:00+09:00", CultureInfo.InvariantCulture),
            EndAt = DateTimeOffset.Parse("2030-02-01T10:00:00+09:00", CultureInfo.InvariantCulture)
        });
        var path = new Uri($"/api/calendar/events/{id}", UriKind.Relative);
        var get = await client.GetAsync(path, TestContext.Current.CancellationToken);
        var etag = get.Headers.ETag!.Tag;
        var request = new CalendarEventUpdateRequest
        {
            CalendarId = calendarId,
            Title = "Version moved",
            AllDay = true,
            StartDate = new DateOnly(2030, 2, 2),
            EndDate = new DateOnly(2030, 2, 3)
        };

        // Act
        using var matched = new HttpRequestMessage(HttpMethod.Put, path);
        matched.Headers.IfMatch.ParseAdd(etag);
        matched.Content = JsonContent.Create(request);
        var updated = await client.SendAsync(matched, TestContext.Current.CancellationToken);

        using var stale = new HttpRequestMessage(HttpMethod.Put, path);
        stale.Headers.IfMatch.ParseAdd(etag);
        stale.Content = JsonContent.Create(request);
        var rejected = await client.SendAsync(stale, TestContext.Current.CancellationToken);

        var latest = await client.GetFromJsonAsync<CalendarEventGetResponse>(path, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        Assert.Equal("\"2\"", updated.Headers.ETag!.Tag);
        Assert.Equal(HttpStatusCode.PreconditionFailed, rejected.StatusCode);
        Assert.NotNull(latest);
        Assert.Equal("Version moved", latest.Title);
        Assert.True(latest.AllDay);
        Assert.Equal(new DateOnly(2030, 2, 2), latest.StartDate);
        Assert.Equal(2, latest.Version);
    }

    [Fact]
    public async Task CreateRejectsInvalidInput()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        var calendarId = await FirstCalendarIdAsync(client);
        var path = new Uri("/api/calendar/events", UriKind.Relative);

        // Act
        var unknownCalendar = await client.PostAsJsonAsync(path, new CalendarEventCreateRequest
        {
            CalendarId = 9999,
            Title = "Unknown",
            StartAt = DateTimeOffset.Parse("2030-03-01T09:00:00+09:00", CultureInfo.InvariantCulture),
            EndAt = DateTimeOffset.Parse("2030-03-01T10:00:00+09:00", CultureInfo.InvariantCulture)
        }, TestContext.Current.CancellationToken);
        var reversed = await client.PostAsJsonAsync(path, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Reversed",
            StartAt = DateTimeOffset.Parse("2030-03-01T10:00:00+09:00", CultureInfo.InvariantCulture),
            EndAt = DateTimeOffset.Parse("2030-03-01T09:00:00+09:00", CultureInfo.InvariantCulture)
        }, TestContext.Current.CancellationToken);
        var allDayWithoutDates = await client.PostAsJsonAsync(path, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "No dates",
            AllDay = true
        }, TestContext.Current.CancellationToken);
        var untitled = await client.PostAsJsonAsync(path, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = string.Empty,
            AllDay = true,
            StartDate = new DateOnly(2030, 3, 1),
            EndDate = new DateOnly(2030, 3, 2)
        }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, unknownCalendar.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, reversed.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, allDayWithoutDates.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, untitled.StatusCode);
        Assert.Equal("application/problem+json", unknownCalendar.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DeleteRemovesEvent()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        var calendarId = await FirstCalendarIdAsync(client);
        var id = await CreateAsync(client, new CalendarEventCreateRequest
        {
            CalendarId = calendarId,
            Title = "Delete",
            AllDay = true,
            StartDate = new DateOnly(2030, 4, 1),
            EndDate = new DateOnly(2030, 4, 2)
        });
        var path = new Uri($"/api/calendar/events/{id}", UriKind.Relative);

        // Act
        var deleted = await client.DeleteAsync(path, TestContext.Current.CancellationToken);
        var get = await client.GetAsync(path, TestContext.Current.CancellationToken);
        var again = await client.DeleteAsync(path, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
    }

    private static async Task<long> FirstCalendarIdAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<CalendarListResponse>(new Uri("/api/calendar/calendars", UriKind.Relative), TestContext.Current.CancellationToken);
        return response!.Items[0].Id;
    }

    private static async Task<long> CreateAsync(HttpClient client, CalendarEventCreateRequest request)
    {
        var response = await client.PostAsJsonAsync(new Uri("/api/calendar/events", UriKind.Relative), request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CalendarEventCreateResponse>(TestContext.Current.CancellationToken))!.Id;
    }
}
