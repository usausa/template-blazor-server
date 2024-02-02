namespace Template.Server.Pages;

using Smart.AspNetCore.Components;

[AllowAnonymous]
public sealed partial class LoginPage
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required LoginManager LoginManager { get; set; }

    public sealed class Form
    {
        [DisplayName("ID")]
        public string Id { get; set; } = default!;

        [DisplayName("Password")]
        public string Password { get; set; } = default!;
    }

    private static readonly IValidator Validator = new InlineValidator<Form>
    {
        v => v.RuleFor(x => x.Id).NotEmpty().MaximumLength(Length.Id),
        v => v.RuleFor(x => x.Password).NotEmpty().MaximumLength(Length.Password)
    };

    private readonly Form form = new();

    private CustomValidator? customValidator;

    private string? errorMessage;

    private bool passwordVisible;

    [Parameter]
    public bool Reload { get; set; }

    private async Task OnValidSubmit()
    {
        errorMessage = null;
        customValidator!.ClearErrors();

        if (await LoginManager.LoginAsync(form.Id, form.Password))
        {
            if (!Reload)
            {
                NavigationManager.NavigateTo("/");
            }
        }
        else
        {
            errorMessage = "Id or Password is invalid.";
            customValidator.DisplayErrors(new Dictionary<string, List<string>>
            {
                { nameof(form.Id), [string.Empty] },
                { nameof(form.Password), [string.Empty] }
            });
        }
    }

    private void TogglePasswordVisibility()
    {
        passwordVisible = !passwordVisible;
    }
}
