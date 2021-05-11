using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace Demo.Logging.Serilog.App
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Console.Title = "Demo APP";

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), @"c:\temp\logs\app-log.json", shared: true)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, $"Host terminated unexpectedly. {e.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog();
                });
    }
}
