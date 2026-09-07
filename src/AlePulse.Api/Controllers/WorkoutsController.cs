using System.Security.Claims;
using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workoutRepository;

    public WorkoutsController(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workout = await _workoutRepository.GetByIdAsync(id);
        if (workout == null || workout.UserId != GetUserId()) return NotFound();
        return Ok(workout);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] AddWorkoutToProgramDto dto)
    {
        var workout = await _workoutRepository.GetByIdAsync(id);
        if (workout == null || workout.UserId != GetUserId()) return NotFound();

        workout.Name = dto.Name;
        workout.Description = dto.Description;

        await _workoutRepository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var workout = await _workoutRepository.GetByIdAsync(id);
        if (workout == null || workout.UserId != GetUserId()) return NotFound();

        await _workoutRepository.DeleteAsync(workout);
        await _workoutRepository.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{workoutId}/exercises")]
    public async Task<IActionResult> AddExercise(Guid workoutId, [FromBody] AddWorkoutExerciseDto dto)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout == null || workout.UserId != GetUserId())
            return NotFound("Treino não encontrado.");

        // NOVA REGRA: Impedir adicionar o mesmo exercício duas vezes no mesmo treino
        var exerciseExists = workout.Exercises.Any(e => e.ExerciseId == dto.ExerciseId);
        if (exerciseExists)
            return Conflict("Este exercício já foi adicionado a este treino.");

        var nextOrder = workout.Exercises.Any() ? workout.Exercises.Max(e => e.Order) + 1 : 1;

        var workoutExercise = new WorkoutExercise
        {
            WorkoutId = workoutId,
            ExerciseId = dto.ExerciseId,
            Sets = dto.Sets,
            Repetitions = dto.Repetitions,
            Weight = dto.Weight,
            RestSeconds = dto.RestSeconds,
            Order = nextOrder
        };

        await _workoutRepository.AddExerciseAsync(workoutExercise);
        await _workoutRepository.SaveChangesAsync();

        return Ok(workoutExercise);
    }

    [HttpPut("{workoutId}/exercises/{exerciseId}")]
    public async Task<IActionResult> UpdateExercise(Guid workoutId, Guid exerciseId, [FromBody] AddWorkoutExerciseDto dto)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout == null || workout.UserId != GetUserId())
            return NotFound("Treino não encontrado.");

        var exercise = workout.Exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            return NotFound("Exercício não encontrado neste treino.");

        exercise.Sets = dto.Sets;
        exercise.Repetitions = dto.Repetitions;
        exercise.Weight = dto.Weight;
        exercise.RestSeconds = dto.RestSeconds;

        await _workoutRepository.UpdateExerciseAsync(exercise);
        return NoContent();
    }

    [HttpDelete("{workoutId}/exercises/{exerciseId}")]
    public async Task<IActionResult> DeleteExercise(Guid workoutId, Guid exerciseId)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout == null || workout.UserId != GetUserId())
            return NotFound("Treino não encontrado.");

        var exerciseToRemove = workout.Exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exerciseToRemove == null)
            return NotFound("Exercício não encontrado neste treino.");

        await _workoutRepository.DeleteExerciseFromWorkoutAsync(exerciseToRemove);
        await _workoutRepository.SaveChangesAsync();

        return NoContent();
    }
}