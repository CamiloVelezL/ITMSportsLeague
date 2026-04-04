using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Intefaces.Repositories;
using SportsLeague.Domain.Intefaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los patrocinadores");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo patrocinador con ID {SponsorId}", id);
            return await _sponsorRepository.GetByIdAsync(id);
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");

            if (!IsValidEmail(sponsor.ContactEmail))
                throw new InvalidOperationException($"El email '{sponsor.ContactEmail}' no tiene un formato válido");

            _logger.LogInformation("Creando patrocinador {Name}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Patrocinador con ID {id} no encontrado");

            var duplicate = await _sponsorRepository.GetByNameAsync(sponsor.Name);
            if (duplicate != null && duplicate.Id != id)
                throw new InvalidOperationException($"Ya existe otro patrocinador con el nombre '{sponsor.Name}'");

            if (!IsValidEmail(sponsor.ContactEmail))
                throw new InvalidOperationException($"El email '{sponsor.ContactEmail}' no es válido");

            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            _logger.LogInformation("Actualizando patrocinador con ID {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"Patrocinador con ID {id} no encontrado");

            _logger.LogInformation("Eliminando patrocinador con ID {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        // ========== Operaciones de vinculación ==========

        public async Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            // Validar que el sponsor existe
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException($"Patrocinador con ID {sponsorId} no encontrado");

            // Validar que el torneo existe
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException($"Torneo con ID {tournamentId} no encontrado");

            if (contractAmount <= 0)
                throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");

            var existing = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing != null)
                throw new InvalidOperationException("Este patrocinador ya está vinculado a este torneo");

            var tournamentSponsor = new TournamentSponsor
            {
                TournamentId = tournamentId,
                SponsorId = sponsorId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);

            // Recuperar la entidad con las navegaciones cargadas (Tournament y Sponsor)
            var createdWithDetails = await _tournamentSponsorRepository.GetByTournamentAndSponsorWithDetailsAsync(tournamentId, sponsorId);
            return createdWithDetails!;
        }

        public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
        {
            var link = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (link == null)
                throw new KeyNotFoundException("La vinculación entre el patrocinador y el torneo no existe");

            _logger.LogInformation("Desvinculando sponsor {SponsorId} de torneo {TournamentId}", sponsorId, tournamentId);
            await _tournamentSponsorRepository.DeleteAsync(link.Id);
        }

        public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
                throw new KeyNotFoundException($"Patrocinador con ID {sponsorId} no encontrado");

            var tournamentSponsors = await _tournamentSponsorRepository.GetBySponsorAsync(sponsorId);
            return tournamentSponsors.Select(ts => ts.Tournament);
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public Task GetLinkWithDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task GetTournamentNameAsync(int tournamentId)
        {
            throw new NotImplementedException();
        }
    }
}