namespace Template.Server.Components.Authentication;

using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class AccountModelBinderProvider : IModelBinderProvider
{
    private static readonly AccountModelBinder Binder = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(Account) ? Binder : null;
    }

    public sealed class AccountModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.Result = ModelBindingResult.Success(bindingContext.HttpContext.User.ToAccount());
            return Task.CompletedTask;
        }
    }
}
