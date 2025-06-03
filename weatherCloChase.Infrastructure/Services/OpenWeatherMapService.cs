using System.Text.Json;
using Microsoft.Extensions.Configuration;
using weatherCloChase.Core.Interfaces;
using weatherCloChase.Core.Models;

namespace weatherCloChase.Infrastructure.Services;

public class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5";

    public OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenWeatherMap:ApiKey"]!;
    }

    public async Task<WeatherData> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var url = $"{_baseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json);
        var root = data.RootElement;

        return new WeatherData
        {
            Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
            FeelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
            Humidity = root.GetProperty("main").GetProperty("humidity").GetDouble(),
            WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
            Description = root.GetProperty("weather")[0].GetProperty("description").GetString()!,
            Icon = root.GetProperty("weather")[0].GetProperty("icon").GetString()!,
            DateTime = DateTime.UtcNow
        };
    }

    public async Task<List<WeatherData>> GetForecastAsync(double latitude, double longitude, int hours = 24)
    {
        var cnt = (hours / 3) + 1; // API возвращает прогноз каждые 3 часа
        var url = $"{_baseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&cnt={cnt}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json);
        var list = data.RootElement.GetProperty("list");

        var forecasts = new List<WeatherData>();
        foreach (var item in list.EnumerateArray())
        {
            forecasts.Add(new WeatherData
            {
                Temperature = item.GetProperty("main").GetProperty("temp").GetDouble(),
                FeelsLike = item.GetProperty("main").GetProperty("feels_like").GetDouble(),
                Humidity = item.GetProperty("main").GetProperty("humidity").GetDouble(),
                WindSpeed = item.GetProperty("wind").GetProperty("speed").GetDouble(),
                Description = item.GetProperty("weather")[0].GetProperty("description").GetString()!,
                Icon = item.GetProperty("weather")[0].GetProperty("icon").GetString()!,
                DateTime = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).UtcDateTime
            });
        }

        return forecasts;
    }
}