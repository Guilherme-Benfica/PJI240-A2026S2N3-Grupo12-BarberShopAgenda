using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BarberShopAgenda.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var chave = Environment.GetEnvironmentVariable("BARBERSHOP_JWT_KEY")
            ?? _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Chave JWT não configurada.");

        var minutos = int.TryParse(_configuration["Jwt:ExpiraMinutos"], out var valor) ? valor : 480;
        var expiraEm = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("usuarioId", usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Papel.ToString()),
        };

        if (usuario.Barbeiro is not null)
            claims.Add(new Claim("barbeiroId", usuario.Barbeiro.Id.ToString()));

        if (usuario.Cliente is not null)
            claims.Add(new Claim("clienteId", usuario.Cliente.Id.ToString()));

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
