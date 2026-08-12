using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Entities;

namespace AlePulse.Application.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateJwtToken(User user);
}