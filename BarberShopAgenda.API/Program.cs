using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BarberShopAgenda.API.Middleware;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using BarberShopAgenda.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";
const string PoliticaLogin = "login";
const string PoliticaPublica = "publico";

// Connection string: variável de ambiente tem prioridade (uso no Azure App Service)
var connectionString = Environment.GetEnvironmentVariable("BARBERSHOP_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string não configurada.");

builder.Services.AddDbContext<BarberShopContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)))
           .AddInterceptors(new Utf8mb4ConnectionInterceptor()));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHorarioDisponivelService, HorarioDisponivelService>();
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();
builder.Services.AddScoped<IClienteContaService, ClienteContaService>();

// Chave JWT: variável de ambiente tem prioridade (mesmo padrão da connection string)
var jwtKey = Environment.GetEnvironmentVariable("BARBERSHOP_JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Chave JWT não configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BarberShop Agenda API",
        Version = "v1",
        Description = "API REST para agendamento de barbearia — clientes, barbeiros, serviços e agendamentos."
    });
});

// Atrás do proxy do Render o IP do cliente vem no X-Forwarded-For; sem isso o rate limit
// enxergaria um IP só para todo mundo e um visitante derrubaria o acesso dos outros.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login e recuperação de senha: janela curta e apertada, contra força bruta.
    options.AddPolicy(PoliticaLogin, contexto => RateLimitPartition.GetFixedWindowLimiter(
        IdentificarChamador(contexto),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(5) }));

    // Endpoints públicos de escrita (autocadastro e agendamento): evita flood de registros.
    options.AddPolicy(PoliticaPublica, contexto => RateLimitPartition.GetFixedWindowLimiter(
        IdentificarChamador(contexto),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(5) }));

    options.OnRejected = async (contexto, cancellationToken) =>
    {
        contexto.HttpContext.Response.ContentType = "application/json";
        await contexto.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(new { mensagem = "Muitas tentativas em pouco tempo. Aguarde alguns minutos e tente de novo." }),
            cancellationToken);
    };
});

static string IdentificarChamador(HttpContext contexto) =>
    contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5500", "http://127.0.0.1:5500" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(frontendOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberShop Agenda API v1");
    });
}

app.UseForwardedHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
