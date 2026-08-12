using AlePulse.Application.Interfaces;
using AlePulse.Application.Services;
using AlePulse.Infrastructure.Persistence;
using AlePulse.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models; // <--- Usings adicionados aqui
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados
builder.Services.AddDbContext<AlePulseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Injeção de Dependência dos Repositórios e Serviços
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Registra os Controllers
builder.Services.AddControllers();

// 4. Configuração do Swagger UI com JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AlePulse.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 5. Configuração do JWT (Autenticação)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_key")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// 6. Habilita o Swagger na aplicação
app.UseSwagger();
app.UseSwaggerUI();

// 7. Diz à API para usar Autenticação e Autorização
app.UseAuthentication();
app.UseAuthorization();

// 8. Diz à API para usar as rotas dos Controllers
app.MapControllers();

app.Run();