using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers; 
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _lineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository lineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _lineupRepository = lineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> AddToLineupAsync(int matchId, MatchLineup lineup)
    {
        // V1: El partido debe existir
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // V6: El partido debe estar en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden registrar alineaciones en partidos Scheduled");

        // V2: El jugador debe existir
        var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);
        if (player == null)
            throw new KeyNotFoundException($"No se encontró el jugador con ID {lineup.PlayerId}");

        // V3: El jugador debe pertenecer al HomeTeam o AwayTeam del partido
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            throw new InvalidOperationException("El jugador no pertenece a ninguno de los equipos del partido");

        // V4: El jugador no puede estar registrado dos veces
        var alreadyExists = await _lineupRepository.ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);
        if (alreadyExists)
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");

        // V5: Máximo 11 titulares por equipo
        if (lineup.IsStarter)
        {
            int startersCount = await _lineupRepository.CountStartersByMatchAndTeamAsync(matchId, player.TeamId);
            if (startersCount >= 11)
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
        }

        lineup.MatchId = matchId;
        _logger.LogInformation("Added player {PlayerId} to lineup for match {MatchId} as {Role}",
            lineup.PlayerId, matchId, lineup.IsStarter ? "starter" : "substitute");

        return await _lineupRepository.CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _lineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // Validar que el teamId corresponda a home o away 
        if (teamId != match.HomeTeamId && teamId != match.AwayTeamId)
            throw new InvalidOperationException("El equipo no participa en este partido");

        return await _lineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task RemoveFromLineupAsync(int lineupId)
    {
        var exists = await _lineupRepository.ExistsAsync(lineupId);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró la alineación con ID {lineupId}");

        await _lineupRepository.DeleteAsync(lineupId);
        _logger.LogInformation("Removed lineup entry {LineupId}", lineupId);
    }
}