using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige que o usuário esteja logado para acessar os exercícios
public class ExercisesController : ControllerBase
{
    private readonly IExerciseRepository _exerciseRepository;

    public ExercisesController(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exercises = await _exerciseRepository.GetAllAsync();
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null) return NotFound();
        return Ok(exercise);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExerciseDto dto)
    {
        var exercise = new Exercise
        {
            Name = dto.Name,
            PrimaryMuscleGroup = dto.PrimaryMuscleGroup,
            SecondaryMuscleGroup = dto.SecondaryMuscleGroup,
            Equipment = dto.Equipment,
            Difficulty = dto.Difficulty,
            Instructions = dto.Instructions,
            IsOfficial = false // Todo exercício criado pelo usuário não é oficial
        };

        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, exercise);
    }
}