using weatherCloChase.Core.Models;

namespace weatherCloChase.Core.Interfaces;

public interface IWeatherService
{
    Task<WeatherData> GetCurrentWeatherAsync(double latitude, double longitude);
    Task<List<WeatherData>> GetForecastAsync(double latitude, double longitude, int hours = 24);
}