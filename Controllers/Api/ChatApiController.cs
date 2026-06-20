using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace GamefiedSelfImprovement.Controllers.Api;

[ApiController]
[Route("api/chat")]
public class ChatApiController : ControllerBase
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "llama-3.1-8b-instant";
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
        _apiKey = configuration["Groq:ApiKey"]
               ?? Environment.GetEnvironmentVariable("Groq__ApiKey")
               ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
               ?? "";
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return BadRequest(new { error = "Poruka ne može biti prazna." });

        if (string.IsNullOrWhiteSpace(_apiKey))
            return StatusCode(503, new { error = "AI asistent trenutno nije dostupan. API ključ nije konfiguriran na serveru." });

        var messages = new List<object>
        {
            new { role = "system", content = SystemPrompt }
        };

        if (request.History != null)
        {
            foreach (var msg in request.History)
            {
                // chatbot.js šalje role:"model" za asistenta — mapiramo u "assistant" za OpenAI format
                var role = msg.Role == "model" ? "assistant" : msg.Role;
                messages.Add(new { role, content = msg.Text });
            }
        }

        messages.Add(new { role = "user", content = request.Message });

        var payload = new
        {
            model = Model,
            messages,
            max_tokens = 1000,
            temperature = 0.7
        };

        var client = _httpClientFactory.CreateClient();
        var json = JsonSerializer.Serialize(payload);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await client.PostAsync(Endpoint, httpContent);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 429)
                return StatusCode(503, new { error = "AI asistent je privremeno nedostupan zbog prekoračenja kvote. Pokušajte za minutu." });
            return StatusCode(500, new { error = "Greška pri komunikaciji s AI-em. Pokušajte ponovno." });
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
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
