namespace Template.BlazorServer.Host.Models.Data;

public sealed record DataListResponse(int Total, int Page, int Size, IReadOnlyList<DataResponse> Items);
