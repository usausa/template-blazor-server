namespace Template.BlazorServer.Host.Application;

using Smart.Mapper;

using Template.BlazorServer.Host.Models.Data;
using Template.BlazorServer.Host.Models.Forms;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataForm ToForm(this DataEntity entity);

    [Mapper]
    public static partial DataResponse ToResponse(this DataEntity entity);
}
