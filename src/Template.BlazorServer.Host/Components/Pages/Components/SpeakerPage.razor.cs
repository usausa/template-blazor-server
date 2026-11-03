namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

using Microsoft.AspNetCore.Components;

public sealed partial class SpeakerPage
{
    private const string Lang = "ja-JP";

    private static readonly Tone[] ConfirmTones = [new(880, 120), new(1320, 160, 40)];

    private static readonly Tone[] ErrorTones = [new(330, 250), new(330, 250, 80)];

    private static readonly Tone[] NotifyTones = [new(660, 120), new(880, 120, 40), new(1100, 200, 40)];

    private IReadOnlyList<SpeechVoice> voices = [];

    private string text = "Components.Blazor の読み上げの例です。文ごとに区切って読み上げます。";

    private string voice = string.Empty;

    private double rate = 1;

    private double pitch = 1;

    private double volume = 1;

    private string toneResult = "-";

    private string speechResult = "-";

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required ISpeaker Speaker { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        Speaker.ReadyChanged += OnSpeakerChanged;
        Speaker.SpeakingChanged += OnSpeakerChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Speaker.PrepareAsync();
            voices = await Speaker.GetVoicesAsync();
            StateHasChanged();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Speaker.ReadyChanged -= OnSpeakerChanged;
            Speaker.SpeakingChanged -= OnSpeakerChanged;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnSpeakerChanged(object? sender, EventArgs e) =>
        _ = InvokeAsync(StateHasChanged);

    private Task PlayConfirmAsync() => PlayAsync(ConfirmTones);

    private Task PlayErrorAsync() => PlayAsync(ErrorTones);

    private Task PlayNotifyAsync() => PlayAsync(NotifyTones);

    private async Task SpeakAsync()
    {
        speechResult = "Speaking";
        var options = new SpeechOptions { Lang = Lang, Voice = String.IsNullOrEmpty(voice) ? null : voice, Rate = rate, Pitch = pitch, Volume = volume };
        speechResult = await Speaker.SpeakAsync(text, options) ? "Completed" : "Canceled";
    }

    private Task CancelAsync() => Speaker.CancelAsync().AsTask();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private async Task PlayAsync(IEnumerable<Tone> tones)
    {
        toneResult = "Playing";
        toneResult = await Speaker.PlayAsync(tones) ? "Played" : "Not played";
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static string VoiceText(SpeechVoice item) =>
        item.LocalService ? $"{item.Name} ({item.Lang})" : $"{item.Name} ({item.Lang}, remote)";

    private static string FormatValue(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);
}
