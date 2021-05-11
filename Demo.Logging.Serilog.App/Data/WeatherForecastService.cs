using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Demo.Logging.Serilog.App.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly HttpClient _client;
        private readonly ILogger<WeatherForecastService> _logger;

        public WeatherForecastService(IHttpClientFactory clientFactory, ILogger<WeatherForecastService> logger)
        {
            this._client = clientFactory.CreateClient("WeatherForecastAPI");
            this._logger = logger;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            this._logger.LogInformation($"Invoked {nameof(GetForecastAsync)} method with: {nameof(startDate)} {startDate}");

            /* var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray()); */

            WeatherForecast[] forecasts = await this._client.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            return forecasts;
        }
    }
}
