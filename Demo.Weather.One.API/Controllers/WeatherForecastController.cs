using Demo.Weather.One.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Weather.One.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService weatherForecastService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService weatherForecastService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.weatherForecastService = weatherForecastService;
            _logger.LogDebug($"Instantiated {nameof(WeatherForecastController)} class.");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("API received {name} request.", nameof(Get));

            var result = await weatherForecastService.GetForecastsAsync(0);

            if (result.Length == 0) 
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            LogContext.PushProperty(nameof(id), id);
            _logger.LogInformation("{name}({id}) has been invoked.", nameof(GetById), id);

            if (id == default)
            {
                var invalidMessage = "Invalid id value";
                _logger.LogInformation(invalidMessage);
                return BadRequest(invalidMessage);
            }

            if (id > 999)
            {
                throw new ArgumentOutOfRangeException($"{nameof(id)} value {id} is out of allowed range.");
            }

            var result = await weatherForecastService.GetForecastsAsync(id);

            if (result.Length == 0) 
            {
                return NotFound();
            }

            for (var i = 0; i < result.Length; i++) 
            {
                result[i].CityName = $"City {id}";
            }

            _logger.LogInformation("{name} processing complete", nameof(GetById));
            return Ok(result);
        }
    }
}
