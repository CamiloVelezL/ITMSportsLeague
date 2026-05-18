using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _lineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService lineupService, IMapper mapper)
    {
        _lineupService = lineupService;
        _mapper = mapper;
    }

    // POST /api/match/{matchId}/lineup

    [HttpPost("lineup")]
    public async Task<ActionResult<MatchLineupResponseDTO>> AddToLineup(int matchId, CreateMatchLineupDTO dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);
            var created = await _lineupService.AddToLineupAsync(matchId, lineup);
            // Recargar con navegación para obtener nombres
            var fullLineup = (await _lineupService.GetLineupByMatchAsync(matchId)).FirstOrDefault(l => l.Id == created.Id);
            return CreatedAtAction(nameof(GetLineupByMatch), new { matchId }, _mapper.Map<MatchLineupResponseDTO>(fullLineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/match/{matchId}/lineup

    [HttpGet("lineup")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByMatch(int matchId)
    {
        try
        {
            var lineups = await _lineupService.GetLineupByMatchAsync(matchId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/match/{matchId}/lineup/team/{teamId}

    [HttpGet("lineup/team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByMatchAndTeam(int matchId, int teamId)
    {
        try
        {
            var lineups = await _lineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/match/{matchId}/lineup/{id}

    [HttpDelete("lineup/{id}")]
    public async Task<ActionResult> RemoveFromLineup(int matchId, int id)
    {
        try
        {
            await _lineupService.RemoveFromLineupAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}