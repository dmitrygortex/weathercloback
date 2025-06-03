using Microsoft.ML;
using Microsoft.ML.Data;
using weatherCloChase.ML.Models;

namespace weatherCloChase.ML.Services;

public class ClothingClassifier
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly string[] _labels = 
    {
        "jacket", "boots", "t-shirt", "dress", 
        "hat", "shirt", "skirt", "sneakers", "bloose"
    };

    public ClothingClassifier(string modelPath)
    {
        _mlContext = new MLContext();
        _model = LoadModel(modelPath);
    }

    private ITransformer LoadModel(string modelPath)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(new List<ImageData>());
        
        var pipeline = _mlContext.Transforms.LoadImages("image", imageFolder: "", nameof(ImageData.ImagePath))
            .Append(_mlContext.Transforms.ResizeImages("image", 512, 512))
            .Append(_mlContext.Transforms.ExtractPixels("input", "image"))
            .Append(_mlContext.Transforms.ApplyOnnxModel(
                modelFile: modelPath,
                outputColumnNames: new[] { "output" },
                inputColumnNames: new[] { "input" }));

        return pipeline.Fit(dataView);
    }

    public async Task<ClothingPrediction> ClassifyImageAsync(Stream imageStream)
    {
        // Сохраняем изображение во временный файл
        var tempPath = Path.GetTempFileName();
        try
        {
            using (var fileStream = File.Create(tempPath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            var imageData = new ImageData { ImagePath = tempPath };
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_model);
            var prediction = predictionEngine.Predict(imageData);

            var probabilities = prediction.Score;
            var maxIndex = probabilities.ToList().IndexOf(probabilities.Max());
            
            var allProbabilities = new Dictionary<string, float>();
            for (int i = 0; i < _labels.Length; i++)
            {
                allProbabilities[_labels[i]] = probabilities[i];
            }

            return new ClothingPrediction
            {
                Category = _labels[maxIndex],
                Confidence = probabilities[maxIndex],
                AllProbabilities = allProbabilities
            };
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private class ImageData
    {
        public string ImagePath { get; set; } = string.Empty;
    }

    private class ImagePrediction
    {
        [ColumnName("output")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }
}