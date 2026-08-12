using System;
using System.Collections.Generic;
using System.Text;

using AlePulse.Domain.Entities;

namespace AlePulse.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navegação (1 para 1)
    public UserProfile? Profile { get; set; }
}