namespace Template.Server.Application.Authentication;

public static class AuthorizeExceptionExtensions
{
    public static T MustExist<T>(this T? value)
    {
        if (value is null)
        {
            throw new NotFoundException();
        }

        return value;
    }
}
