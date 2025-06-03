namespace weatherCloChase.ML.Models;

public class ClothingPrediction
{
    public string Category { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, float> AllProbabilities { get; set; } = new();
}