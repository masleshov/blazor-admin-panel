using Newtonsoft.Json;

namespace BlazorAdminPanel.External.Response
{
    public record PostPreviewResponseDto
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; init; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; init; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; init; }

        [JsonProperty(PropertyName = "likes")]
        public int Likes { get; init; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; init; }

        [JsonProperty(PropertyName = "publishDate")]
        public DateTime PublishDate { get; init; }

        [JsonProperty(PropertyName = "owner")]
        public UserPreviewResponseDto Owner { get; init; }
    }
}
