using Newtonsoft.Json;

namespace BlazorAdminPanel.External.Response
{
    public record UserPreviewResponseDto
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; init; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; init; }

        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; init; }

        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; init; }

        [JsonProperty(PropertyName = "picture")]
        public string Picture { get; init; }
    }
}
