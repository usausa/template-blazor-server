namespace Template.BlazorServer.Host.Mappers;

using Smart.Mapper;

using Template.BlazorServer.Host.Models.Data;
using Template.BlazorServer.Host.Models.Forms;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataForm ToForm(DataEntity entity);

    [Mapper]
    public static partial DataResponse ToResponse(DataEntity entity);
}
