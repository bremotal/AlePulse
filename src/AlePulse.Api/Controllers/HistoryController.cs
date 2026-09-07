using System.Security.Claims;
using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IWorkoutSessionRepository _sessionRepository;
    private readonly AlePulseDbContext _context; // Injeção do banco de dados

    public HistoryController(IWorkoutSessionRepository sessionRepository, AlePulseDbContext context)
    {
        _sessionRepository = sessionRepository;
        _context = context;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
    }

    [HttpGet("{exerciseId}")]
    public async Task<IActionResult> GetHistory(Guid exerciseId)
    {
        var history = await _sessionRepository.GetHistoryByExerciseAsync(GetUserId(), exerciseId);
        return Ok(history);
    }

    [HttpPost("{workoutId}/{exerciseId}")]
    public async Task<IActionResult> LogSet(Guid workoutId, Guid exerciseId, [FromBody] LogSetDto dto)
    {
        await _sessionRepository.LogSetAsync(GetUserId(), workoutId, exerciseId, dto);
        return Ok(new { message = "Série registrada com sucesso!" });
    }

    [HttpPut("{setId}")]
    public async Task<IActionResult> UpdateSet(Guid setId, [FromBody] LogSetDto dto)
    {
        try
        {
            if (dto == null) return BadRequest("Dados inválidos.");

            await _sessionRepository.UpdateSetAsync(setId, dto.SetNumber, dto.Weight, dto.Repetitions);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpDelete("{setId}")]
    public async Task<IActionResult> DeleteSet(Guid setId)
    {
        try
        {
            await _sessionRepository.DeleteSetAsync(setId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet("completed-today/{workoutId}")]
    public async Task<IActionResult> GetCompletedExercisesToday(Guid workoutId)
    {
        var userId = GetUserId();
        var today = DateTime.UtcNow.Date;

        var setsToday = await _context.ExerciseSets
            .Where(es => es.WorkoutSession.WorkoutId == workoutId
                      && es.WorkoutSession.UserId == userId
                      && es.CompletedAt.Value.Date == today).Distinct()
            .ToListAsync();

        return Ok(setsToday);
    }
}