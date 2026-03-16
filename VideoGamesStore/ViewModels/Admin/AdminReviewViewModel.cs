namespace VideoGamesStore.ViewModels.Admin;

public class AdminReviewViewModel
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
}
