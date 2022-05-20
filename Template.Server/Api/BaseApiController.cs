namespace Template.Server.Api;

using Template.Server.Infrastructure.Filters;

[Area("api")]
[Route("[area]/[controller]/[action]")]
[ApiController]
[ApiExceptionFilter]
public class BaseApiController : ControllerBase
{
}
