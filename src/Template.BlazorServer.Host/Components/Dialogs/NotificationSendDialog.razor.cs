namespace Template.BlazorServer.Host.Components.Dialogs;

using FluentValidation;

using Microsoft.AspNetCore.Components;

using MudBlazor;

public sealed partial class NotificationSendDialog
{
    private const string AllTarget = "*";

    private static readonly NotificationFormValidator Validator = new();

#pragma warning disable CA2213
    private MudForm form = default!;
#pragma warning restore CA2213

    private readonly NotificationForm model = new();

    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    public required IReadOnlyList<string> Names { get; set; }

    [CascadingParameter]
    public required IMudDialogInstance MudDialog { get; set; }

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    private async Task OnOkClick()
    {
        await form.ValidateAsync();
        if (form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(new NotificationSendRequest(
                model.Target == AllTarget ? null : model.Target,
                model.Title,
                model.Body,
                String.IsNullOrEmpty(model.Link) ? null : model.Link)));
        }
    }

    private void OnCancelClick() => MudDialog.Cancel();

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    internal sealed class NotificationForm
    {
        public string Target { get; set; } = AllTarget;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;
    }

    internal sealed class NotificationFormValidator : AbstractValidator<NotificationForm>
    {
        public NotificationFormValidator()
        {
            RuleFor(static x => x.Title)
                .NotEmpty().WithMessage("題を入力してください。")
                .MaximumLength(Length.NotificationTitle).WithMessage($"題は{Length.NotificationTitle}文字以内で入力してください。");
            RuleFor(static x => x.Body)
                .NotEmpty().WithMessage("本文を入力してください。")
                .MaximumLength(Length.NotificationBody).WithMessage($"本文は{Length.NotificationBody}文字以内で入力してください。");
            RuleFor(static x => x.Link)
                .Must(NotificationLink.IsValid).WithMessage("リンクはアプリの中のパス(例: data)で入力してください。");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<NotificationForm>.CreateWithOptions((NotificationForm)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(static e => e.ErrorMessage);
        };
    }
}
