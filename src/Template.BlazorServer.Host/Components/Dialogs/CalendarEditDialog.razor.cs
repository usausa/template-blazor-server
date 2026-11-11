namespace Template.BlazorServer.Host.Components.Dialogs;

using FluentValidation;

using Microsoft.AspNetCore.Components;

using MudBlazor;

public sealed partial class CalendarEditDialog
{
    private static readonly Dictionary<string, string> ColorNames = new(StringComparer.Ordinal)
    {
        [CalendarColors.Blue] = "青",
        [CalendarColors.Green] = "緑",
        [CalendarColors.Red] = "赤",
        [CalendarColors.Orange] = "橙",
        [CalendarColors.Yellow] = "黄",
        [CalendarColors.Purple] = "紫"
    };

    private static readonly CalendarFormValidator Validator = new();

#pragma warning disable CA2213
    private MudForm form = default!;
#pragma warning restore CA2213

    private CalendarForm model = default!;

    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public CalendarEntity? Entity { get; set; }

    [CascadingParameter]
    public required IMudDialogInstance MudDialog { get; set; }

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        model = new CalendarForm
        {
            Name = Entity?.Name ?? string.Empty,
            Color = Entity?.Color ?? CalendarColors.Blue
        };
    }

    private async Task OnOkClick()
    {
        await form.ValidateAsync();
        if (form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(new CalendarEntity
            {
                Id = Entity?.Id ?? 0,
                Name = model.Name.Trim(),
                Color = model.Color,
                Version = Entity?.Version ?? 0
            }));
        }
    }

    private void OnCancelClick() => MudDialog.Cancel();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private string ColorIcon(string color) =>
        color == model.Color ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Circle;

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    internal sealed class CalendarForm
    {
        public string Name { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;
    }

    internal sealed class CalendarFormValidator : AbstractValidator<CalendarForm>
    {
        public CalendarFormValidator()
        {
            RuleFor(static x => x.Name)
                .NotEmpty().WithMessage("名前を入力してください。")
                .MaximumLength(Length.Name).WithMessage($"名前は{Length.Name}文字以内で入力してください。");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<CalendarForm>.CreateWithOptions((CalendarForm)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(static e => e.ErrorMessage);
        };
    }
}
