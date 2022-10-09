using BlazorAdminPanel.Configuration;
using Microsoft.Extensions.Options;

namespace BlazorAdminPanel.External
{
    public class UserHttpHandler : DelegatingHandler
    {
        private readonly ExternalConfiguration _configuration;

        public UserHttpHandler(IOptions<ExternalConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            requestMessage.Headers.Add("app-id", _configuration.AppId);
            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}
