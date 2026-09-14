using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using AlePulse.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly AlePulseDbContext _context;

    public WorkoutsController(
        IWorkoutRepository workoutRepository,
        AlePulseDbContext context)
    {
        _workoutRepository = workoutRepository;
        _context = context;
    }

    // ============================================================
    // USUÁRIO LOGADO
    // ============================================================

    private Guid GetUserId()
    {
        var claim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        return claim != null
            ? Guid.Parse(claim.Value)
            : Guid.Empty;
    }


    // ============================================================
    // OBTER TREINO
    // ============================================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(id);

        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound();
        }

        return Ok(workout);
    }


    // ============================================================
    // ATUALIZAR TREINO
    // ============================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkout(
        Guid id,
        [FromBody] AddWorkoutToProgramDto dto)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(id);

        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound();
        }

        workout.Name =
            dto.Name;

        workout.Description =
            dto.Description;

        await _workoutRepository.SaveChangesAsync();

        return NoContent();
    }


    // ============================================================
    // EXCLUIR TREINO
    // ============================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(id);

        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound();
        }

        await _workoutRepository.DeleteAsync(workout);

        await _workoutRepository.SaveChangesAsync();

        return NoContent();
    }


    // ============================================================
    // ADICIONAR EXERCÍCIO AO TREINO
    // ============================================================

    [HttpPost("{workoutId}/exercises")]
    public async Task<IActionResult> AddExercise(
        Guid workoutId,
        [FromBody] AddWorkoutExerciseDto dto)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(workoutId);

        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound(
                "Treino não encontrado.");
        }


        var exerciseToAdd =
            await _context.Exercises.FindAsync(
                dto.ExerciseId);

        if (exerciseToAdd == null)
        {
            return NotFound(
                "Exercício não encontrado na biblioteca.");
        }


        var nameExists =
            workout.Exercises.Any(
                e =>
                    e.Exercise != null &&
                    e.Exercise.Name.ToLower()
                    ==
                    exerciseToAdd.Name.ToLower());


        if (nameExists)
        {
            return Conflict(
                "Você já adicionou um exercício com este nome neste treino.");
        }


        // ========================================================
        // PRÓXIMA POSIÇÃO
        // ========================================================

        var nextOrder =
            workout.Exercises.Any()
                ? workout.Exercises.Max(e => e.Order) + 1
                : 1;


        var workoutExercise =
            new WorkoutExercise
            {
                WorkoutId = workoutId,

                ExerciseId = dto.ExerciseId,

                Sets = dto.Sets,

                Repetitions = dto.Repetitions,

                Weight = dto.Weight,

                RestSeconds = dto.RestSeconds,

                Order = nextOrder
            };


        await _workoutRepository
            .AddExerciseAsync(workoutExercise);

        await _workoutRepository
            .SaveChangesAsync();


        return Ok(workoutExercise);
    }


    // ============================================================
    // REORDENAR EXERCÍCIOS
    // ============================================================
    //
    // Recebe:
    //
    // {
    //     "exerciseIds": [
    //         "id-exercicio-3",
    //         "id-exercicio-1",
    //         "id-exercicio-2"
    //     ]
    // }
    //
    // E grava:
    //
    // exercício 3 -> Order = 1
    // exercício 1 -> Order = 2
    // exercício 2 -> Order = 3
    //
    // ============================================================

    [HttpPut("{workoutId}/exercises/reorder")]
    public async Task<IActionResult> ReorderExercises(
        Guid workoutId,
        [FromBody] ReorderDto dto)
    {
        // ========================================================
        // VALIDAR DTO
        // ========================================================

        if (dto == null ||
            dto.ExerciseIds == null ||
            dto.ExerciseIds.Count == 0)
        {
            return BadRequest(
                "A lista de exercícios não pode estar vazia.");
        }


        // ========================================================
        // LOCALIZAR TREINO
        // ========================================================

        var workout =
            await _workoutRepository.GetByIdAsync(workoutId);


        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound(
                "Treino não encontrado.");
        }


        // ========================================================
        // EXERCÍCIOS DO TREINO
        // ========================================================

        var workoutExercises =
            workout.Exercises.ToList();


        // ========================================================
        // VALIDAR QUANTIDADE
        // ========================================================

        if (dto.ExerciseIds.Count !=
            workoutExercises.Count)
        {
            return BadRequest(
                "A quantidade de exercícios recebida não corresponde aos exercícios do treino.");
        }


        // ========================================================
        // VALIDAR DUPLICADOS
        // ========================================================

        if (dto.ExerciseIds.Distinct().Count() !=
            dto.ExerciseIds.Count)
        {
            return BadRequest(
                "A lista contém exercícios duplicados.");
        }


        // ========================================================
        // VALIDAR SE TODOS OS IDs PERTENCEM AO TREINO
        // ========================================================

        var workoutExerciseIds =
            workoutExercises
                .Select(e => e.Id)
                .ToHashSet();


        var invalidIds =
            dto.ExerciseIds
                .Where(id => !workoutExerciseIds.Contains(id))
                .ToList();


        if (invalidIds.Count > 0)
        {
            return BadRequest(
                "Um ou mais exercícios não pertencem a este treino.");
        }


        // ========================================================
        // GRAVAR NOVA ORDEM
        // ========================================================

        for (int i = 0;
             i < dto.ExerciseIds.Count;
             i++)
        {
            var exerciseId =
                dto.ExerciseIds[i];


            var exercise =
                workoutExercises.First(
                    e => e.Id == exerciseId);


            exercise.Order =
                i + 1;
        }


        // ========================================================
        // SALVAR NO BANCO
        // ========================================================

        await _workoutRepository
            .SaveChangesAsync();


        // ========================================================
        // RETORNO
        // ========================================================

        return NoContent();
    }


    // ============================================================
    // ATUALIZAR EXERCÍCIO
    // ============================================================

    [HttpPut("{workoutId}/exercises/{exerciseId}")]
    public async Task<IActionResult> UpdateExercise(
        Guid workoutId,
        Guid exerciseId,
        [FromBody] AddWorkoutExerciseDto dto)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(workoutId);

        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound(
                "Treino não encontrado.");
        }


        var exercise =
            workout.Exercises.FirstOrDefault(
                e => e.Id == exerciseId);


        if (exercise == null)
        {
            return NotFound(
                "Exercício não encontrado neste treino.");
        }


        exercise.Sets =
            dto.Sets;

        exercise.Repetitions =
            dto.Repetitions;

        exercise.Weight =
            dto.Weight;

        exercise.RestSeconds =
            dto.RestSeconds;


        await _workoutRepository
            .UpdateExerciseAsync(exercise);


        return NoContent();
    }


    // ============================================================
    // EXCLUIR EXERCÍCIO DO TREINO
    // ============================================================

    [HttpDelete("{workoutId}/exercises/{exerciseId}")]
    public async Task<IActionResult> DeleteExercise(
        Guid workoutId,
        Guid exerciseId)
    {
        var workout =
            await _workoutRepository.GetByIdAsync(workoutId);


        if (workout == null ||
            workout.UserId != GetUserId())
        {
            return NotFound(
                "Treino não encontrado.");
        }


        var exerciseToRemove =
            workout.Exercises.FirstOrDefault(
                e => e.Id == exerciseId);


        if (exerciseToRemove == null)
        {
            return NotFound(
                "Exercício não encontrado neste treino.");
        }


        await _workoutRepository
            .DeleteExerciseFromWorkoutAsync(
                exerciseToRemove);


        await _workoutRepository
            .SaveChangesAsync();


        return NoContent();
    }
}