using System.Security.Claims;
using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly AlePulseDbContext _context; // Injeção do banco para buscar o nome do exercício

    public WorkoutsController(IWorkoutRepository workoutRepository, AlePulseDbContext context)
    {
        _workoutRepository = workoutRepository;
        _context = context;
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

        // Busca o nome do exercício que está tentando adicionar
        var exerciseToAdd = await _context.Exercises.FindAsync(dto.ExerciseId);
        if (exerciseToAdd == null) return NotFound("Exercício não encontrado na biblioteca.");

        // REGRA: Verifica se já existe um exercício com o MESMO NOME no treino (ignora maiúsculas/minúsculas)
        var nameExists = workout.Exercises.Any(e => e.Exercise != null &&
            e.Exercise.Name.ToLower() == exerciseToAdd.Name.ToLower());

        if (nameExists)
            return Conflict("Você já adicionou um exercício com este nome neste treino.");

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
   
    [HttpPut("{workoutId}/exercises/{exerciseId}/move")]
    public async Task<IActionResult> MoveExercise(Guid workoutId, Guid exerciseId, [FromQuery] string direction)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout == null || workout.UserId != GetUserId()) return NotFound("Treino não encontrado.");

        var current = workout.Exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (current == null) return NotFound("Exercício não encontrado.");

        var orderedExercises = workout.Exercises.OrderBy(e => e.Order).ToList();
        var currentIndex = orderedExercises.IndexOf(current);

        WorkoutExercise? swapTarget = null;
        if (direction == "up" && currentIndex > 0)
            swapTarget = orderedExercises[currentIndex - 1];
        else if (direction == "down" && currentIndex < orderedExercises.Count - 1)
            swapTarget = orderedExercises[currentIndex + 1];

        if (swapTarget != null)
        {
            var tempOrder = current.Order;
            current.Order = swapTarget.Order;
            swapTarget.Order = tempOrder;
            await _workoutRepository.SaveChangesAsync();
        }

        return Ok();
    }

}