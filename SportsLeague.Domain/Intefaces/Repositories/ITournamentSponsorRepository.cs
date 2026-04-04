using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Intefaces.Repositories
{
    public interface ITournamentSponsorRepository : IGenericRepository<TournamentSponsor>
    {
        Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId);
        Task<IEnumerable<TournamentSponsor>> GetByTournamentAsync(int tournamentId);
        Task<IEnumerable<TournamentSponsor>> GetBySponsorAsync(int sponsorId);
        Task<TournamentSponsor?> GetByTournamentAndSponsorWithDetailsAsync(int tournamentId, int sponsorId);
    }
}
