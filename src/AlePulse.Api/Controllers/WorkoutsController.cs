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

    // Pega o ID do usuário logado direto do Token JWT
    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMyWorkouts()
    {
        var userId = GetUserId();
        var workouts = await _workoutRepository.GetAllByUserIdAsync(userId);
        return Ok(workouts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workout = await _workoutRepository.GetByIdAsync(id);
        if (workout == null || workout.UserId != GetUserId())
            return NotFound("Treino não encontrado ou não pertence a você.");

        return Ok(workout);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutDto dto)
    {
        var workout = new Workout
        {
            Name = dto.Name,
            Description = dto.Description,
            UserId = GetUserId()
        };

        await _workoutRepository.AddAsync(workout);
        await _workoutRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout);
    }

    [HttpPost("{workoutId}/exercises")]
    public async Task<IActionResult> AddExercise(Guid workoutId, [FromBody] AddWorkoutExerciseDto dto)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout == null || workout.UserId != GetUserId())
            return NotFound("Treino não encontrado ou não pertence a você.");

        // Pega a próxima ordem do exercício na ficha
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
}