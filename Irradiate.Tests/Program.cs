using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Irradiate.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "benchmark")
            {
                var summary = BenchmarkRunner.Run<Benchmark>();
                Console.WriteLine(summary);
                return;
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:5000");
                });
    }
}
