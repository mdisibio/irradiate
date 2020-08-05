using System;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Sampling.Local;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Irradiate.Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Hard-code xray to trace everything.
            AWSXRayRecorder.InitializeInstance(recorder:
                new AWSXRayRecorderBuilder()
                    //.WithDaemonAddress(Environment.GetEnvironmentVariable("AWS_XRAY_DAEMON_ADDRESS"))
                    .WithSamplingStrategy(new LocalizedSamplingStrategy()
                    {
                        DefaultRule = new SamplingRule
                        {
                            Rate = 1.0,
                            FixedTarget = 1000,
                        }
                    }).Build());

            services
                .AddSingleton<IWeatherForecastRepo, WeatherForecastRepo>()
                .Irradiate<IWeatherForecastRepo>();

            

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseXRay("test");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
