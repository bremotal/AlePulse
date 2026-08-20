using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
            IsOfficial = false
        };

        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, exercise);
    }

    // ENDPOINT ATUALIZADO: Receber upload de imagem do celular
    [HttpPost("{id}/media/upload")]
    public async Task<IActionResult> UploadMedia(Guid id, IFormFile file)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null) return NotFound("Exercício não encontrado.");

        if (file == null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        // Cria a pasta 'uploads' se não existir
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Gera um nome único para o arquivo (ex: 1234-abcd.gif)
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Salva o arquivo no disco
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Cria a URL para acessar a imagem (ex: http://localhost:5204/uploads/1234-abcd.gif)
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var imageUrl = $"{baseUrl}/uploads/{uniqueFileName}";

        // Salva a URL no banco de dados
        var media = new ExerciseMedia
        {
            ExerciseId = id,
            Url = imageUrl,
            MediaType = MediaType.Image
        };

        await _exerciseRepository.AddMediaAsync(media);
        await _exerciseRepository.SaveChangesAsync();

        return Ok(new { message = "Imagem enviada com sucesso!", url = imageUrl });
    }
}