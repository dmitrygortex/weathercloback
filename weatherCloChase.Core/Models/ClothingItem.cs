namespace weatherCloChase.Core.Models;

public class ClothingItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty; // текстом типы потом энам сделать
    public string ImageUrl { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public User User { get; set; } = null!;
}