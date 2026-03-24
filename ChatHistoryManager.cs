using System.Text;
using System.Text.Json;

namespace GamefiedSelfImprovement;

public class ChatHistoryManager
{
    private List<ChatMessage> _messages = new();
    private readonly string _historyFilePath;

    // Hooks/Events
    public event Action<ChatMessage>? OnMessageAdded;
    public event Action<List<ChatMessage>>? OnHistoryExported;
    public event Action? OnHistoryCleared;

    public ChatHistoryManager(string? filePath = null)
    {
        _historyFilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ChatHistory.json"
        );
        LoadHistory();
    }

    /// <summary>
    /// Dodaj novu poruku u istoriju
    /// </summary>
    public void AddMessage(string role, string content, string? model = null)
    {
        var message = new ChatMessage(role, content, model);
        _messages.Add(message);
        OnMessageAdded?.Invoke(message);
        SaveHistory();
    }

    /// <summary>
    /// Preuzmi sve poruke
    /// </summary>
    public List<ChatMessage> GetHistory() => new(_messages);

    /// <summary>
    /// Preuzmi poruke u određenom rasponu
    /// </summary>
    public List<ChatMessage> GetHistory(int skip, int take) 
        => _messages.Skip(skip).Take(take).ToList();

    /// <summary>
    /// Preuzmi posljednje N poruka
    /// </summary>
    public List<ChatMessage> GetLastMessages(int count) 
        => _messages.TakeLast(count).ToList();

    /// <summary>
    /// Exportuj istoriju u JSON format
    /// </summary>
    public string ExportAsJson()
    {
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true 
        };
        var json = JsonSerializer.Serialize(_messages, options);
        OnHistoryExported?.Invoke(_messages);
        return json;
    }

    /// <summary>
    /// Exportuj istoriju u TXT format
    /// </summary>
    public string ExportAsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== CHAT HISTORY ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total messages: {_messages.Count}");
        sb.AppendLine(new string('=', 50));
        sb.AppendLine();

        foreach (var msg in _messages)
        {
            sb.AppendLine($"[{msg.Timestamp:yyyy-MM-dd HH:mm:ss}] {msg.Role.ToUpper()}:");
            if (!string.IsNullOrEmpty(msg.Model))
                sb.AppendLine($"   Model: {msg.Model}");
            sb.AppendLine($"   {msg.Content}");
            sb.AppendLine();
        }

        OnHistoryExported?.Invoke(_messages);
        return sb.ToString();
    }

    /// <summary>
    /// Snimi istoriju na disk
    /// </summary>
    public void SaveHistory()
    {
        try
        {
            var json = ExportAsJson();
            File.WriteAllText(_historyFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri snimanju istorije: {ex.Message}");
        }
    }

    /// <summary>
    /// Učitaj istoriju sa diska
    /// </summary>
    public void LoadHistory()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                var json = File.ReadAllText(_historyFilePath);
                _messages = JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri učitavanju istorije: {ex.Message}");
        }
    }

    /// <summary>
    /// Očisti istoriju
    /// </summary>
    public void ClearHistory()
    {
        _messages.Clear();
        OnHistoryCleared?.Invoke();
        SaveHistory();
    }

    /// <summary>
    /// Exportuj istoriju direktno u file
    /// </summary>
    public void ExportToFile(string filePath, ExportFormat format = ExportFormat.Json)
    {
        try
        {
            var content = format == ExportFormat.Json ? ExportAsJson() : ExportAsText();
            File.WriteAllText(filePath, content);
            Console.WriteLine($"✓ Istorija exportana u: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri exportu: {ex.Message}");
        }
    }

    /// <summary>
    /// Preuzmi broj poruka u istoriji
    /// </summary>
    public int MessageCount => _messages.Count;

    /// <summary>
    /// Preuzmi put do istorije file-a
    /// </summary>
    public string HistoryPath => _historyFilePath;
}

public enum ExportFormat
{
    Json,
    Text
}
