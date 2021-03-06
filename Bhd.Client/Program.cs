using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bhd.Client.Services;
using Bhd.Client.SignalR;
using MudBlazor;

namespace Bhd.Client {
    public class Program {
        public static async Task Main(string[] args) {
#if DEBUG
            await Task.Delay(2000);
#endif

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IRestService, JsonRestService>();

            builder.Services.AddSingleton<NotificationsHub>();
            builder.Services.AddSingleton<PageHeaderService>();
            builder.Services.AddMudServices(config => {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            });

            await builder.Build().RunAsync();
        }
    }
}
