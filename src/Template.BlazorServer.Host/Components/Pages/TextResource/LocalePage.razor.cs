namespace Template.BlazorServer.Host.Components.Pages.TextResource;

using FluentValidation;

using Microsoft.AspNetCore.Components;

using MudBlazor;

public sealed partial class LocalePage
{
    private static readonly CultureInfo JapaneseCulture = CultureInfo.GetCultureInfo(Cultures.Japanese);

    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(Cultures.English);

    private static readonly LocaleFormValidator Validator = new();

#pragma warning disable CA2213
    private MudForm form = default!;
#pragma warning restore CA2213

    private readonly LocaleForm model = new();

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required NavigationManager Navigation { get; set; }

    private static string CultureText =>
        $"CurrentCulture: {CultureInfo.CurrentCulture.Name} / CurrentUICulture: {CultureInfo.CurrentUICulture.Name}";

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    private void ChangeCulture(string name) => Navigation.NavigateTo($"culture/{name}", forceLoad: true);

    private Task ValidateAsync() => form.ValidateAsync();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static Variant LanguageVariant(string name) =>
        CultureInfo.CurrentUICulture.Name == name ? Variant.Filled : Variant.Outlined;

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    internal sealed class LocaleForm
    {
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class LocaleFormValidator : AbstractValidator<LocaleForm>
    {
        public LocaleFormValidator()
        {
            RuleFor(static x => x.Name)
                .NotEmpty().WithMessage(static _ => Messages.NameRequired)
                .MaximumLength(Length.Name).WithMessage(static _ => Messages.NameMaxLength(Length.Name));
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<LocaleForm>.CreateWithOptions((LocaleForm)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(static e => e.ErrorMessage);
        };
    }
}
