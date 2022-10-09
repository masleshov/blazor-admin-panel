using Newtonsoft.Json;

namespace BlazorAdminPanel.External.Response
{
    public record ListResponseDto<TModel>
    {
        [JsonProperty(PropertyName = "data")]
        public TModel[] Data { get; init; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; init; }

        [JsonProperty(PropertyName = "page")]
        public int Page { get; init; }

        [JsonProperty(PropertyName = "limit")]
        public int Limit { get; init; }
    }
}
