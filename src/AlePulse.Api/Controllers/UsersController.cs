using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Application.Services;
using AlePulse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    // Método privado para extrair o ID do usuário do Token JWT
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
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

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _userRepository.GetByIdAsync(GetUserId());
        if (user == null) return NotFound();

        return Ok(new { user.Name, user.Email });
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(GetUserId());
        if (user == null) return NotFound();

        // Verifica se o e-mail novo não pertence a outra pessoa
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null && existingUser.Id != user.Id)
            return Conflict("Este e-mail já está em uso por outra conta.");

        user.Name = dto.Name;
        user.Email = dto.Email;

        await _userRepository.UpdateUserAsync(user);
        return NoContent();
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(GetUserId());
        if (user == null) return NotFound();

        if (!_authService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return BadRequest("Senha atual incorreta.");

        user.PasswordHash = _authService.HashPassword(dto.NewPassword);
        await _userRepository.UpdateUserAsync(user);

        return NoContent();
    }
}