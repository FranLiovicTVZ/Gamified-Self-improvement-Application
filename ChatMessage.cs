using System;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

public class ChatMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty; // "user" ili "assistant"

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; } // Opciono: koji model je odgovorio

    public ChatMessage() 
    {
        Role = "unknown";
    }

    public ChatMessage(string role, string content, string? model = null)
    {
        Role = role;
        Content = content;
        Model = model;
    }
}
