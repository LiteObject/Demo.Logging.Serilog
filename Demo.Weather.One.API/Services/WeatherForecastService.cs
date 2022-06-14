using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Demo.Weather.One.API.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly ILogger<WeatherForecastService> _logger;
        private readonly HttpClient _httpClient;

        /* private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        }; */

        public WeatherForecastService(ILogger<WeatherForecastService> logger, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _logger.LogDebug($"Instantiated {nameof(WeatherForecastService)} class.");
        }

        public async Task<WeatherForecast[]> GetForecastsAsync(int id)
        {
            _logger.LogInformation("{method} has been invoked.", nameof(GetForecastsAsync));

            /* var rng = new Random();
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                CityName = $"City-{id}",
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            _logger.LogInformation("{method}({param}) processing complete.", nameof(GetForecasts), id); */

            var responseString = await _httpClient.GetStringAsync("WeatherForecast");
            var result = System.Text.Json.JsonSerializer.Deserialize<WeatherForecast[]>(responseString);

            return result;
        }
    }
}
