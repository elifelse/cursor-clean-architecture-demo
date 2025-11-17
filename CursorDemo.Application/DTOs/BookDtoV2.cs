namespace CursorDemo.Application.DTOs;

/// <summary>
/// Book DTO for API version 2.0 - includes additional fields
/// </summary>
public class BookDtoV2
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Version { get; set; } = "2.0";
}

