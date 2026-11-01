namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

using Microsoft.AspNetCore.Components;

public sealed partial class ChatPage
{
    private const string UserId = "user";

    private const string AgentId = "agent";

    private readonly IgbChatOptions options = new()
    {
        CurrentUserId = UserId,
        HeaderText = "Support",
        InputPlaceholder = "Type a message",
        Suggestions = ["What is Ignite UI?", "Which license is it under?"]
    };

    private IgbChatMessage[] messages = [];

    private int messageId;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required TimeProvider TimeProvider { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        messages = [NewMessage(AgentId, "Hello. Send a message and the agent replies with the same text.")];
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private async Task OnMessageCreatedAsync(IgbChatMessageEventArgs args)
    {
        var message = args.Detail;
        if (messages.All(x => x.Id != message.Id))
        {
            messages = [.. messages, message];
        }

        options.Suggestions = [];
        options.IsTyping = true;

        await Task.Delay(700);

        options.IsTyping = false;
        messages = [.. messages, NewMessage(AgentId, $"You said: {message.Text}")];
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private IgbChatMessage NewMessage(string sender, string text) =>
        new()
        {
            Id = (++messageId).ToString(CultureInfo.InvariantCulture),
            Sender = sender,
            Text = text,
            Timestamp = TimeProvider.GetUtcNow().ToString("o", CultureInfo.InvariantCulture)
        };
}
