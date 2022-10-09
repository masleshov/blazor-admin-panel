namespace BlazorAdminPanel.Configuration
{
    public sealed record AuthConfiguration
    {
        public string Login { get; init; }
        public string Password { get; init; }
    }
}
