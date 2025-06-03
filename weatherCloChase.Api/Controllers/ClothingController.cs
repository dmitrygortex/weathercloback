using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using weatherCloChase.Core.Interfaces;
using weatherCloChase.Core.Models;
using weatherCloChase.Infrastructure.Data;
using weatherCloChase.ML.Services;

namespace weatherCloChase.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
//[Authorize]
public class ClothingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly ClothingClassifier _classifier;
    private readonly ILogger<ClothingController> _logger;

    public ClothingController(
        ApplicationDbContext context,
        IStorageService storageService,
        ClothingClassifier classifier,
        ILogger<ClothingController> logger)
    {
        _context = context;
        _storageService = storageService;
        _classifier = classifier;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadClothing([FromForm] IFormFile image)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "No image provided" });

            // Классифицируем изображение
            using var imageStream = image.OpenReadStream();
            var prediction = await _classifier.ClassifyImageAsync(imageStream);
            
            if (prediction.Confidence < 0.7f)
                return BadRequest(new { error = "Cannot determine clothing type with confidence" });

            // Сбрасываем позицию потока
            imageStream.Position = 0;
            
            // Загружаем в S3
            var s3Key = await _storageService.UploadImageAsync(
                imageStream, 
                image.FileName, 
                image.ContentType);

            // Сохраняем в БД
            var clothingItem = new ClothingItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = prediction.Category,
                S3Key = s3Key,
                ImageUrl = _storageService.GetPublicUrl(s3Key),
                UploadedAt = DateTime.UtcNow
            };

            _context.ClothingItems.Add(clothingItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = clothingItem.Id,
                category = clothingItem.Category,
                imageUrl = clothingItem.ImageUrl,
                confidence = prediction.Confidence,
                uploadedAt = clothingItem.UploadedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading clothing");
            return StatusCode(500, new { error = "Failed to upload image" });
        }
    }

    [HttpGet("my-wardrobe")]
    public async Task<IActionResult> GetMyWardrobe()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        
        var items = await _context.ClothingItems
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UploadedAt)
            .Select(c => new
            {
                id = c.Id,
                category = c.Category,
                imageUrl = c.ImageUrl,
                uploadedAt = c.UploadedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClothing(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        
        var item = await _context.ClothingItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (item == null)
            return NotFound(new { error = "Clothing item not found" });

        try
        {
            // Удаляем из S3
            await _storageService.DeleteImageAsync(item.S3Key);
            
            // Удаляем из БД
            _context.ClothingItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Clothing item deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting clothing item");
            return StatusCode(500, new { error = "Failed to delete clothing item" });
        }
    }
}