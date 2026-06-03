using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Predstavlja priloženu datoteku vezanu uz aktivnost ili korisnika
/// </summary>
public class Attachment
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("activityId")]
    public int? ActivityId { get; set; }

    [ForeignKey("ActivityId")]
    [JsonPropertyName("activity")]
    public virtual Activity? Activity { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    [JsonPropertyName("user")]
    public virtual AppUser? User { get; set; }

    [Required(ErrorMessage = "Naziv datoteke je obavezan")]
    [StringLength(256, ErrorMessage = "Naziv datoteke ne smije biti duži od 256 znakova")]
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Putanja do datoteke je obavezna")]
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "MIME tip ne smije biti duži od 100 znakova")]
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("fileSize")]
    [Range(0, long.MaxValue, ErrorMessage = "Veličina datoteke ne smije biti negativna")]
    public long FileSize { get; set; }

    [JsonPropertyName("uploadedDate")]
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    [StringLength(500, ErrorMessage = "Opis ne smije biti duži od 500 znakova")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    public Attachment() { }

    public Attachment(string fileName, string filePath, string contentType, long fileSize)
    {
        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedDate = DateTime.UtcNow;
    }

    public override string ToString() => 
        $"📎 {FileName} ({FileSize / 1024}KB) - {UploadedDate:dd.MM.yyyy HH:mm}";
}
