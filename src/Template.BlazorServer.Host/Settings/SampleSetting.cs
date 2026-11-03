namespace Template.BlazorServer.Host.Settings;

public sealed class SampleSetting
{
    [Range(1, Int64.MaxValue)]
    public long UploadLimit { get; set; } = 1024L * 1024 * 1024;
}
