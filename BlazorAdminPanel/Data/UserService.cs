using BlazorAdminPanel.External;
using BlazorAdminPanel.External.Response;

namespace BlazorAdminPanel.Data
{
    public class UserService
    {
        private readonly IUserExternalClient _userClient;

        public UserService(IUserExternalClient userClient)
        {
            _userClient = userClient;
        }

        public async Task<ListResponseDto<UserPreviewResponseDto>> GetUserPreviewList(int limit, int page)
        {
            return await _userClient.GetUsers(limit, page);
        }

        public async Task<ListResponseDto<PostPreviewResponseDto>> GetUserPosts(string userId, int limit, int page)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(userId);
            }

            return await _userClient.GetUserPosts(userId, limit, page);
        }

        private async Task<List<TData>> GetPagedData<TData>(Func<int, int, Task<ListResponseDto<TData>>> externalApiCall)
        {
            const int limit = 50;
            int page = 0;
            int total = int.MaxValue;

            var result = new List<TData>();
            do
            {
                var response = await externalApiCall(limit, page++);
                if (total == int.MaxValue) total = response.Total;

                result.AddRange(response.Data);
            }
            while (limit * page < total);

            return result;
        }
    }
}
