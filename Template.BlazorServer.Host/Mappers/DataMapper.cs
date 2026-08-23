namespace Template.BlazorServer.Host.Mappers;

using Smart.Mapper;

using Template.BlazorServer.Host.Models.Forms;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataForm ToForm(DataEntity entity);
}
