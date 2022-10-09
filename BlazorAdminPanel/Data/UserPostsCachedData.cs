using BlazorAdminPanel.External.Response;

namespace BlazorAdminPanel.Data
{
    public class UserPostsCachedData
    {
        public int Count;
        public Dictionary<int, CachedPage<PostPreviewResponseDto>> Data = new Dictionary<int, CachedPage<PostPreviewResponseDto>>();
    }
}
