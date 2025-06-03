namespace weatherCloChase.Core.Models;

public class WeatherData
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
}