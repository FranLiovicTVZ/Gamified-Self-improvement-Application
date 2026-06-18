namespace GamefiedSelfImprovement.DTOs;

public class SearchItemDTO
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class GlobalSearchResultDTO
{
    public string Query { get; set; } = string.Empty;
    public List<SearchItemDTO> Results { get; set; } = new();
    public int TotalCount { get; set; }
}
