using BlazorHomieDashboard.Server.Hubs;
using BlazorHomieDashboard.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorHomieDashboard.Server {
    public class Startup {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSingleton<IMqttBroker, ReliableMqttBroker>();
            services.AddSingleton(provider => {
                var broker = provider.GetService<IMqttBroker>();
                var hub = provider.GetService<IHubContext<MqttHub>>();
                var messagePusher = new MessagePusher(broker, hub);
                return messagePusher;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            // Making sure pusher gets created.
            app.ApplicationServices.GetService<MessagePusher>();
            
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<MqttHub>("/mqttHub");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}