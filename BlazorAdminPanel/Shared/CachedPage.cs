using System.Collections.Generic;

namespace Stargazer.Web.UI.Utils
{
    public record CachedPage<TItem>
    {
        public List<TItem> Items { get; init; }
        public long ExpiryTime { get; init; }
    }
}