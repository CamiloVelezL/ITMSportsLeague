using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Intefaces.Repositories
{
    public interface ISponsorRepository : IGenericRepository<Sponsor>
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<Sponsor?> GetByNameAsync(string name);
    }
}
