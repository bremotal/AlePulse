using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Application.DTOs;

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}