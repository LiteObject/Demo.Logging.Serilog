using Demo.Weather.One.API.Middleware;
using Demo.Weather.One.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Demo.Weather.One.API.Configurations;

namespace Demo.Weather.One.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo.Weather.One.API", Version = "v1" });
            });

            // services.Configure<WeatherServiceTwoConfiguration>("ServiceTwo", Configuration.GetSection("Demo.Weather.Two.API"));
            
            services.AddOptions<WeatherServiceTwoConfiguration>()
                .Bind(Configuration.GetSection(WeatherServiceTwoConfiguration.ConfigurationSectionName))
                .ValidateDataAnnotations();

            /*
             * Another great way to configure our options validation is by using delegates. 
             * services.AddOptions<TYPE>().Bind(Configuration.GetSection(...)).ValidateDataAnnotations().Validate(config => {return true;});
             */

            WeatherServiceTwoConfiguration weatherServiceTwoConfiguration = new();
            Configuration.GetSection(WeatherServiceTwoConfiguration.ConfigurationSectionName)
                .Bind(weatherServiceTwoConfiguration);

            // TODD: Add it to an extension method (for IConfiguration)
            // following "OptionsBuilderDataAnnotationsExtensions.ValidateDataAnnotations()" example.
            var context = new ValidationContext(weatherServiceTwoConfiguration, serviceProvider: null, items: null);
            Validator.ValidateObject(weatherServiceTwoConfiguration, context);

            // services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            // Typed Clients (Service Agent pattern), which is the structured way to use IHttpClientFactory
            services.AddHttpClient<IWeatherForecastService, WeatherForecastService>(client => {
                var uriBuilder = new UriBuilder(weatherServiceTwoConfiguration.Host);                
                uriBuilder.Port = weatherServiceTwoConfiguration.Port;
                uriBuilder.Scheme = Uri.UriSchemeHttps;
                client.BaseAddress = uriBuilder.Uri;                
            })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());

            /*
             * Each time you get an HttpClient object from the IHttpClientFactory, a new instance is returned. 
             * But each HttpClient uses an HttpMessageHandler that's pooled and reused by the IHttpClientFactory 
             * to reduce resource consumption, as long as the HttpMessageHandler's lifetime hasn't expired.
             */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<LogContextEnrichment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo.Weather.One.API v1"));
            }

            // app.UseHttpsRedirection();

            // Write streamlined request completion events, instead of the more verbose ones from the framework.
            // To use the default framework request logging instead, remove this line and set the "Microsoft"
            // level in appsettings.json to "Information".
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.BadGateway)
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (response, calculatedWaitDuration, context) =>
                    {
                        Log.Warning($"Failed attempt. Waited for {calculatedWaitDuration}. Retrying. {response.Exception.Message} - {response.Exception.StackTrace}");
                    });
        }
    }
}
