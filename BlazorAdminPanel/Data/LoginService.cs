using BlazorAdminPanel.Configuration;
using Microsoft.Extensions.Options;

namespace BlazorAdminPanel.Data
{
    public class LoginService
    {
        private readonly AuthConfiguration _configuration;

        public LoginService(IOptions<AuthConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public bool Authenticate(string login, string password)
        {
            return login == _configuration.Login
                && password == _configuration.Password;
        }
    }
}
