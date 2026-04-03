using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Intefaces.Repositories
{
    public interface ITournamentRepository : IGenericRepository<Tournament>
    {
        Task<IEnumerable<Tournament>> GetByStatusAsync(TournamentStatus status);
        Task<Tournament?> GetByIdWithTeamsAsync(int id);
    }
}
