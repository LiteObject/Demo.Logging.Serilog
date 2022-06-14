using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Logz.Io;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Weather.Two.API
{
    public class Program
    {
        public static async Task Main(String[] args)
        {
            Console.Title = "Demo API Two";

            IConfiguration configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            var loggerFactory = LoggerFactory
            .Create(builder =>
            {
                builder.ClearProviders();
                // builder.AddConsole();

                var outputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3} {Properties}]{NewLine}{Message:lj}{NewLine}{Exception}";
                var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Grayscale);

                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });

            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogTrace($"Inside the {nameof(Main)} method of {nameof(Program)} class.");

            var host = Host.CreateDefaultBuilder()
            .UseSerilog((hostContext, services, configuration) => {

                // configuration.WriteTo.Console();
                ConfigureSerilog(hostContext, services, configuration);
            })
            //.ConfigureLogging(loggingBuilder => {
            //    loggingBuilder.AddSerilog();
            //})
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup(context => new Startup(configuration, logger));
            })
            .Build();

            await host.RunAsync();
        }

        private static void ConfigureSerilog(HostBuilderContext hostContext, IServiceProvider services, LoggerConfiguration configuration) 
        {
            /*
            * [17:40:59 INF {SourceContext="Serilog.AspNetCore.RequestLoggingMiddleware", RequestId="0HMI5L6SPMRTS:00000005", ConnectionId="0HMI5L6SPMRTS", ThreadId=9, MachineName="RIENGLW139", EnvironmentName="Development", application="demo-app"}]
            */
            //var outputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3} {RequestId} {SourceContext}]{NewLine}>>> {Message:lj}{NewLine}{Exception}";
            var outputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3} {Properties}]{NewLine}{Message:lj}{NewLine}{Exception}";
            // var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";
            // var expressionTemplate = new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3} {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}");
            // var expressionTemplate = new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3} ({SourceContext})]\n{@m}\n{@x}");

            configuration
            //.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            //.MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("application", "demo-app-two") //Globally-attached properties                    
            /* PERFORMANCE:
             * Console logging is synchronous and this can cause bottlenecks in some deployment scenarios. 
             * For high-volume console logging, consider using Serilog.Sinks.Async to move console writes 
             * to a background thread:
             */
            //.WriteTo.Async(wt => wt.Console())
            .WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Code)
            .WriteTo.Debug(new JsonFormatter(renderMessage: true), LogEventLevel.Verbose)
            .WriteTo.LogzIoDurableHttp(
                "https://listener.logz.io:8071?type=dev&token=IponWRfXqQYPbTQDZOqCQDZwCuaIQjlF",
                logzioTextFormatterOptions: new LogzioTextFormatterOptions
                {
                    BoostProperties = true,
                    IncludeMessageTemplate = true,
                    LowercaseLevel = true
                });
        }
    }
}
