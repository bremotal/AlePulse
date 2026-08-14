using System.Security.Claims;
using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IWorkoutSessionRepository _sessionRepository;

    public HistoryController(IWorkoutSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
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
            // Retorna o erro exato para o aplicativo
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
            // Retorna o erro exato para o aplicativo
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }
}