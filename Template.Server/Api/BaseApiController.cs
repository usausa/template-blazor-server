namespace Template.Server.Api;

[Area("api")]
[Microsoft.AspNetCore.Mvc.Route("[area]/[controller]/[action]")]
[ApiController]
public abstract class BaseApiController : ControllerBase
{
}
