namespace BlazorAdminPanel.Configuration
{
    public sealed record ExternalConfiguration
    {
        public string AppId { get; init; }
        public string ApiUrl { get; init; }
    }
}
