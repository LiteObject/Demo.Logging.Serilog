using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Weather.One.API.Services
{
    public interface IWeatherForecastService
    {
        public Task<WeatherForecast[]> GetForecastsAsync(int id);
    }
}
