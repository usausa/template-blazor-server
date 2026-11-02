namespace Template.BlazorServer.Accessors;

using Template.BlazorServer.Infrastructure.Data;

[AccessorProfile]
[TypeHandler(typeof(DateTimeTextConverter))]
public static class DataProfile;
