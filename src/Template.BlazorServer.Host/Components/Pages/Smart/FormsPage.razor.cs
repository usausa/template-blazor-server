namespace Template.BlazorServer.Host.Components.Pages.Smart;

using global::Smart.AspNetCore.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

public sealed partial class FormsPage
{
    private FormModel model = new();

    private EditContext editContext = default!;

#pragma warning disable CA2213
    private CustomValidator validator = default!;
#pragma warning restore CA2213

    private string? result;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required DataService DataService { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized() =>
        editContext = new EditContext(model);

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private async Task SubmitAsync()
    {
        var names = (await DataService.QueryAllAsync()).Select(static x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (names.Contains(model.Name!))
        {
            validator.DisplayError(nameof(FormModel.Name), "This name is used in Data.");
            result = "Rejected by the server";
            return;
        }

        result = $"Submitted: {model.Name} / {model.Email}";
    }

    private void FillSample()
    {
        model.Name = "Sample";
        model.Email = "sample";
        editContext.NotifyFieldsChanged(nameof(FormModel.Name), nameof(FormModel.Email));
        result = "Filled by code";
    }

    private void Clear()
    {
        model = new FormModel();
        editContext = new EditContext(model);
        result = null;
    }

    //--------------------------------------------------------------------------------
    // Model
    //--------------------------------------------------------------------------------

    private sealed class FormModel
    {
        [Required]
        [MaxLength(20)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
