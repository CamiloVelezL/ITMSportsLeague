using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Intefaces.Repositories
{
    public interface ITournamentTeamRepository : IGenericRepository<TournamentTeam>
    {
        Task<TournamentTeam?> GetByTournamentAndTeamAsync(int tournamentId, int teamId);
        Task<IEnumerable<TournamentTeam>> GetByTournamentAsync(int tournamentId);
    }
}
