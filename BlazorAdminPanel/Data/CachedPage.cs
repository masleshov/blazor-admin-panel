namespace BlazorAdminPanel.Data
{
    public record CachedPage<TItem>
    {
        public TItem[] Items { get; init; }
        public long ExpiryTime { get; init; }
    }
}
