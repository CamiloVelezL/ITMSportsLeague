using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Intefaces.Services
{
    public interface ISponsorService
    {
        // CRUD
        Task<IEnumerable<Sponsor>> GetAllAsync();
        Task<Sponsor?> GetByIdAsync(int id);
        Task<Sponsor> CreateAsync(Sponsor sponsor);
        Task UpdateAsync(int id, Sponsor sponsor);
        Task DeleteAsync(int id);

        // Operaciones de vinculación (N:M)
        Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount);
        Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId);
        Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId);
        Task GetLinkWithDetailsAsync(int id);
        Task GetTournamentNameAsync(int tournamentId);
    }
}
