using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutProgramsController : ControllerBase
{
    private readonly IWorkoutProgramRepository _programRepository;

    public WorkoutProgramsController(IWorkoutProgramRepository programRepository)
    {
        _programRepository = programRepository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // 1. Listar todas as Fichas do usuário
    [HttpGet]
    public async Task<IActionResult> GetMyPrograms()
    {
        var programs = await _programRepository.GetAllByUserIdAsync(GetUserId());
        return Ok(programs);
    }

    // 2. Ver uma Ficha específica (com seus Treinos A, B, C e Exercícios)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var program = await _programRepository.GetByIdAsync(id);
        if (program == null || program.UserId != GetUserId())
            return NotFound("Ficha não encontrada.");

        return Ok(program);
    }

    // 3. Criar uma nova Ficha (Ex: "Hipertrofia ABC")
    [HttpPost]
    public async Task<IActionResult> CreateProgram([FromBody] CreateWorkoutProgramDto dto)
    {
        var program = new WorkoutProgram
        {
            Name = dto.Name,
            Description = dto.Description,
            UserId = GetUserId()
        };

        await _programRepository.AddAsync(program);
        await _programRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = program.Id }, program);
    }

    // 4. Adicionar um Treino (Ex: "Treino A") dentro de uma Ficha
    [HttpPost("{programId}/workouts")]
    public async Task<IActionResult> AddWorkoutToProgram(Guid programId, [FromBody] AddWorkoutToProgramDto dto)
    {
        var program = await _programRepository.GetByIdAsync(programId);
        if (program == null || program.UserId != GetUserId())
            return NotFound("Ficha não encontrada.");

        // NOVA REGRA: Impedir treinos com nome duplicado
        if (await _programRepository.WorkoutNameExistsForUserAsync(GetUserId(), dto.Name))
            return Conflict("Já existe um treino com este nome.");

        var workout = new Workout
        {
            Name = dto.Name,
            Description = dto.Description,
            UserId = GetUserId(),
            WorkoutProgramId = programId
        };

        await _programRepository.AddWorkoutAsync(workout);
        await _programRepository.SaveChangesAsync();

        return Ok(workout);
    }


    // Editar Ficha
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] CreateWorkoutProgramDto dto)
    {
        var program = await _programRepository.GetByIdAsync(id);
        if (program == null || program.UserId != GetUserId()) return NotFound();

        program.Name = dto.Name;
        program.Description = dto.Description;

        await _programRepository.UpdateAsync(program);
        return NoContent();
    }

    // Excluir Ficha
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProgram(Guid id)
    {
        var program = await _programRepository.GetByIdAsync(id);
        if (program == null || program.UserId != GetUserId()) return NotFound();

        await _programRepository.DeleteAsync(program);
        await _programRepository.SaveChangesAsync();
        return NoContent();
    }
}