using BlazorAdminPanel.External.Response;
using Refit;

namespace BlazorAdminPanel.External
{
    public interface IUserExternalClient
    {
        [Get("/user")]
        Task<ListResponseDto<UserPreviewResponseDto>> GetUsers(int limit, int page);

        [Get("/user/{id}/post")]
        Task<ListResponseDto<PostPreviewResponseDto>> GetUserPosts([AliasAs("id")] string userId, int limit, int page);
    }
}
