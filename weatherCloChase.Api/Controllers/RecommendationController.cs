using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using weatherCloChase.Core.Interfaces;
using weatherCloChase.Infrastructure.Data;
using weatherCloChase.ML.Services;

namespace weatherCloChase.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
//[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWeatherService _weatherService;
    private readonly ClothingRecommendationService _recommendationService;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(
        ApplicationDbContext context,
        IWeatherService weatherService,
        ClothingRecommendationService recommendationService,
        ILogger<RecommendationController> logger)
    {
        _context = context;
        _weatherService = weatherService;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpPost("get-recommendations")]
    public async Task<IActionResult> GetRecommendations([FromBody] LocationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Получаем текущую погоду и прогноз
            var currentWeather = await _weatherService.GetCurrentWeatherAsync(request.Latitude, request.Longitude);
            var forecast = await _weatherService.GetForecastAsync(request.Latitude, request.Longitude, 12);

            // Получаем гардероб пользователя
            var userWardrobe = await _context.ClothingItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Получаем рекомендации для текущей погоды
            var currentRecommendation = _recommendationService.GetRecommendation(currentWeather, userWardrobe);

            // Анализируем прогноз для дополнительных рекомендаций
            var minTemp = forecast.Min(f => f.Temperature);
            var maxTemp = forecast.Max(f => f.Temperature);
            var willRain = forecast.Any(f => f.Description.Contains("rain"));

            var additionalRecommendations = new List<string>();
            
            if (maxTemp - minTemp > 10)
            {
                additionalRecommendations.Add("Температура будет сильно меняться в течение дня. Возьмите дополнительную одежду.");
            }

            if (willRain && !currentWeather.Description.Contains("rain"))
            {
                additionalRecommendations.Add("Ожидается дождь. Не забудьте зонт!");
            }

            return Ok(new
            {
                currentWeather = new
                {
                    temperature = currentWeather.Temperature,
                    feelsLike = currentWeather.FeelsLike,
                    description = currentWeather.Description,
                    humidity = currentWeather.Humidity,
                    windSpeed = currentWeather.WindSpeed
                },
                recommendation = new
                {
                    description = currentRecommendation.Description,
                    recommendedCategories = currentRecommendation.RecommendedCategories,
                    specificItems = currentRecommendation.SpecificItems.Select(item => new
                    {
                        id = item.Id,
                        category = item.Category,
                        imageUrl = item.ImageUrl
                    }),
                    additionalItems = currentRecommendation.AdditionalItems
                },
                forecast = new
                {
                    minTemperature = minTemp,
                    maxTemperature = maxTemp,
                    willRain = willRain,
                    hourlyForecast = forecast.Take(4).Select(f => new
                    {
                        time = f.DateTime,
                        temperature = f.Temperature,
                        description = f.Description
                    })
                },
                additionalRecommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, new { error = "Failed to get recommendations" });
        }
    }
}

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}