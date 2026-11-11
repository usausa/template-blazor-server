namespace Template.BlazorServer.Host.Components.Dialogs;

using FluentValidation;

using Microsoft.AspNetCore.Components;

using MudBlazor;

public sealed partial class CalendarEventDialog
{
    private static readonly TimeSpan DefaultStartTime = TimeSpan.FromHours(9);

    private static readonly TimeSpan DefaultEndTime = TimeSpan.FromHours(10);

    private static readonly CalendarEventFormValidator Validator = new();

#pragma warning disable CA2213
    private MudForm form = default!;
#pragma warning restore CA2213

    private CalendarEventForm model = default!;

    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public required CalendarEventInput Input { get; set; }

    [Parameter]
    public required IReadOnlyList<CalendarEntity> Calendars { get; set; }

    [CascadingParameter]
    public required IMudDialogInstance MudDialog { get; set; }

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        model = new CalendarEventForm
        {
            Title = Input.Title,
            CalendarId = Input.CalendarId,
            AllDay = Input.AllDay,
            StartDate = Input.Start.Date,
            StartTime = Input.AllDay ? DefaultStartTime : Input.Start.TimeOfDay,
            EndDate = Input.AllDay ? Input.End.Date.AddDays(-1) : Input.End.Date,
            EndTime = Input.AllDay ? DefaultEndTime : Input.End.TimeOfDay,
            Description = Input.Description
        };
    }

    private async Task OnOkClick()
    {
        await form.ValidateAsync();
        if (!form.IsValid)
        {
            return;
        }

        var start = model.AllDay ? model.StartDate!.Value.Date : model.StartDate!.Value.Date + model.StartTime!.Value;
        var end = model.AllDay ? model.EndDate!.Value.Date.AddDays(1) : model.EndDate!.Value.Date + model.EndTime!.Value;
        var description = String.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        MudDialog.Close(DialogResult.Ok(new CalendarEventInput(model.Title.Trim(), model.CalendarId, start, end, model.AllDay, description)));
    }

    private void OnCancelClick() => MudDialog.Cancel();

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    internal sealed class CalendarEventForm
    {
        public string Title { get; set; } = string.Empty;

        public long CalendarId { get; set; }

        public bool AllDay { get; set; }

        public DateTime? StartDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string? Description { get; set; }
    }

    internal sealed class CalendarEventFormValidator : AbstractValidator<CalendarEventForm>
    {
        public CalendarEventFormValidator()
        {
            RuleFor(static x => x.Title)
                .NotEmpty().WithMessage("タイトルを入力してください。")
                .MaximumLength(Length.CalendarEventTitle).WithMessage($"タイトルは{Length.CalendarEventTitle}文字以内で入力してください。");
            RuleFor(static x => x.StartDate)
                .NotNull().WithMessage("開始日を入力してください。");
            RuleFor(static x => x.EndDate)
                .NotNull().WithMessage("終了日を入力してください。")
                .Must(static (form, _) => IsOrdered(form)).WithMessage("終了は開始より後にしてください。").When(static x => x.AllDay, ApplyConditionTo.CurrentValidator);
            RuleFor(static x => x.StartTime)
                .NotNull().WithMessage("開始時刻を入力してください。")
                .When(static x => !x.AllDay);
            RuleFor(static x => x.EndTime)
                .NotNull().WithMessage("終了時刻を入力してください。")
                .Must(static (form, _) => IsOrdered(form)).WithMessage("終了は開始より後にしてください。")
                .When(static x => !x.AllDay);
            RuleFor(static x => x.Description)
                .MaximumLength(Length.CalendarEventDescription).WithMessage($"説明は{Length.CalendarEventDescription}文字以内で入力してください。");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<CalendarEventForm>.CreateWithOptions((CalendarEventForm)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(static e => e.ErrorMessage);
        };

        private static bool IsOrdered(CalendarEventForm form)
        {
            if ((form.StartDate is not { } startDate) || (form.EndDate is not { } endDate))
            {
                return true;
            }

            if (form.AllDay)
            {
                return endDate.Date >= startDate.Date;
            }

            if ((form.StartTime is not { } startTime) || (form.EndTime is not { } endTime))
            {
                return true;
            }

            return endDate.Date + endTime > startDate.Date + startTime;
        }
    }
}
