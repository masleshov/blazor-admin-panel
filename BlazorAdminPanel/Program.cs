using BlazorAdminPanel.Configuration;
using BlazorAdminPanel.Data;
using BlazorAdminPanel.External;
using Microsoft.Extensions.Http.Logging;
using Stargazer.Web.UI.Utils;

namespace BlazorAdminPanel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddScoped(sp => new LoggingHttpMessageHandler(sp.GetRequiredService<ILogger<LoggingHttpMessageHandler>>()));
            builder.Services.AddScoped<BlazorServiceAccessor>();

            var authSection = builder.Configuration.GetSection("Auth");
            var authConfiguration = authSection.Get<AuthConfiguration>();
            builder.Services.Configure<AuthConfiguration>(authSection);

            var externalSection = builder.Configuration.GetSection("External");
            var externalConfiguration = externalSection.Get<ExternalConfiguration>();
            builder.Services.Configure<ExternalConfiguration>(externalSection);

            builder.Services.AddTransient<UserHttpHandler>();
            builder.Services.AddRefitClient<IUserExternalClient>(new Uri(externalConfiguration.ApiUrl))
                .AddHttpMessageHandler<UserHttpHandler>();

            builder.Services.AddTransient<UserService>();
            builder.Services.AddTransient<LoginService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}