using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Application.Services;
using AlePulse.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AlePulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public UsersController(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict("E-mail já cadastrado.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            // Agora a senha é criptografada antes de ir para o banco!
            PasswordHash = _authService.HashPassword(dto.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("E-mail ou senha inválidos.");

        var token = _authService.GenerateJwtToken(user);
        return Ok(new { token });
    }
}