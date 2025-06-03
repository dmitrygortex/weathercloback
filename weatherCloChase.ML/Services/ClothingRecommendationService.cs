using weatherCloChase.Core.Models;

namespace weatherCloChase.ML.Services;

public class ClothingRecommendationService
{
    public ClothingRecommendation GetRecommendation(WeatherData weather, List<ClothingItem> userWardrobe)
    {
        var recommendation = new ClothingRecommendation
        {
            Temperature = weather.Temperature,
            WeatherDescription = weather.Description
        };

        // Логика рекомендаций на основе погоды
        if (weather.Temperature < 5)
        {
            // Холодная погода
            recommendation.RecommendedCategories.AddRange(new[] { "jacket", "boots", "hat", "pants" });
            recommendation.Description = "Холодно! Рекомендуется теплая одежда.";
        }
        else if (weather.Temperature < 15)
        {
            // Прохладная погода
            recommendation.RecommendedCategories.AddRange(new[] { "pants", "sneakers" });
            if (weather.WindSpeed > 5)
            {
                recommendation.RecommendedCategories.Add("jacket");
            }
            recommendation.Description = "Прохладно. Возьмите с собой куртку.";
        }
        else if (weather.Temperature < 25)
        {
            // Теплая погода
            recommendation.RecommendedCategories.AddRange(new[] { "t-shirt", "pants", "sneakers", "dress" });
            recommendation.Description = "Комфортная температура для легкой одежды.";
        }
        else
        {
            // Жаркая погода
            recommendation.RecommendedCategories.AddRange(new[] { "t-shirt", "shorts", "sneakers", "bloose" });
            recommendation.Description = "Жарко! Выбирайте легкую одежду.";
        }

        // Проверка на дождь
        if (weather.Description.Contains("rain"))
        {
            recommendation.AdditionalItems.Add("Зонт или дождевик");
        }

        // Фильтруем только те вещи, которые есть у пользователя
        var availableClothes = userWardrobe
            .Where(item => recommendation.RecommendedCategories.Contains(item.Category))
            .ToList();

        recommendation.SpecificItems = availableClothes;

        return recommendation;
    }
}

public class ClothingRecommendation
{
    public double Temperature { get; set; }
    public string WeatherDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RecommendedCategories { get; set; } = new();
    public List<string> AdditionalItems { get; set; } = new();
    public List<ClothingItem> SpecificItems { get; set; } = new();
}