using AlePulse.Application.Interfaces;
using AlePulse.Infrastructure.Persistence;
using AlePulse.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados
builder.Services.AddDbContext<AlePulseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Injeção de Dependência dos Repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3. Registra os Controllers
builder.Services.AddControllers();

// 4. Configuração do Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Habilita o Swagger na aplicação
app.UseSwagger();
app.UseSwaggerUI();

// 6. Diz à API para usar as rotas dos Controllers
app.MapControllers();

app.Run();