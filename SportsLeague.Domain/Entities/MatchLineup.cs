using SportsLeague.Domain.Entities;
namespace SportsLeague.Domain.Entities;
public class MatchLineup : AuditBase
{
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public bool IsStarter { get; set; }   // true = titular, false = suplente
    public string Position { get; set; } = string.Empty;  // "GK", "CB", "ST", etc.

    // Navigation properties
    public Match Match { get; set; } = null!;
    public Player Player { get; set; } = null!;
}