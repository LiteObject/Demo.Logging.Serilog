using System.ComponentModel.DataAnnotations;

namespace Demo.Weather.One.API.Configurations
{
    /// <summary>
    /// 
    /// For more info on options:
    /// https://docs.microsoft.com/en-us/dotnet/core/extensions/options
    /// </summary>
    public class WeatherServiceTwoConfiguration
    {
        public const string ConfigurationSectionName = "Services:Demo.Weather.Two.API";

        [Required]
        public string Host { get; set; }

        [Required]
        [Range(5000, 9999, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Port { get; set; }
    }
}
