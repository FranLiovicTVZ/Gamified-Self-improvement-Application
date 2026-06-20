using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace GamefiedSelfImprovement.Controllers.Api;

[ApiController]
[Route("api/chat")]
public class ChatApiController : ControllerBase
{
    private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent";
    private const string SystemPrompt =
        "Ti si AI wellness asistent specijaliziran isključivo za teme meditacije i vježbanja. " +
        "Pomažeš korisnicima savjetima o tehnikama meditacije (mindfulness, disanje, vizualizacija, body scan), " +
        "planovima vježbanja, pravilnoj tehnici, oporavku mišića, prehrani za sportaše i mentalnoj dobrobiti. " +
        "Ako te netko pita o temama izvan meditacije i vježbanja, ljubazno objasni da si specijaliziran samo za te teme. " +
        "Odgovaraj na jeziku na kojem te korisnik pita (hrvatski ili engleski). " +
        "Budi prijatan, motivirajući, konkretan i praktičan. Daj strukturirane odgovore kada je prikladno.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public ChatApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["Gemini:ApiKey"] ?? "";
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return BadRequest(new { error = "Poruka ne može biti prazna." });

        if (string.IsNullOrWhiteSpace(_apiKey))
            return StatusCode(503, new { error = "AI asistent trenutno nije dostupan. Gemini API ključ nije konfiguriran na serveru." });

        var contents = new List<object>();

        if (request.History != null)
        {
            foreach (var msg in request.History)
            {
                contents.Add(new
                {
                    role = msg.Role,
                    parts = new[] { new { text = msg.Text } }
                });
            }
        }

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = request.Message } }
        });

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents,
            generationConfig = new { maxOutputTokens = 1000, temperature = 0.7 }
        };

        var client = _httpClientFactory.CreateClient();
        var json = JsonSerializer.Serialize(payload);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{Endpoint}?key={_apiKey}", httpContent);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 429)
                return StatusCode(503, new { error = "AI asistent je privremeno nedostupan zbog prekoračenja dnevne kvote. Pokušajte malo kasnije." });
            return StatusCode(500, new { error = $"Greška pri komunikaciji s AI-em ({(int)response.StatusCode}). Pokušajte ponovno." });
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "Nema odgovora.";

        return Ok(new { reply = text });
    }
}

public class ChatRequest
{
    public string Message { get; set; } = "";
    public List<ChatHistoryMessage>? History { get; set; }
}

public class ChatHistoryMessage
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = "";
}
