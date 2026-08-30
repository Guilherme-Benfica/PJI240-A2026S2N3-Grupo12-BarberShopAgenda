using System.Security.Cryptography;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BarberShopAgenda.Infrastructure.Services;

public class ClienteContaService : IClienteContaService
{
    private const int MinutosExpiracaoVerificacao = 24 * 60;
    private const int MinutosExpiracaoResetSenha = 60;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public ClienteContaService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<Usuario> RegistrarAsync(string nome, string telefone, string email, string senha)
    {
        var existente = await _usuarioRepository.GetByEmailAsync(email);
        if (existente is not null)
            throw new RegraNegocioException("Já existe uma conta com esse e-mail.");

        var cliente = await _clienteRepository.GetByTelefoneAsync(telefone);
        if (cliente is null)
        {
            cliente = await _clienteRepository.AddAsync(new Cliente
            {
                Nome = nome,
                Telefone = telefone,
                Email = email,
                DataCadastro = DateTime.UtcNow
            });
        }

        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            Papel = PapelUsuario.Cliente,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            EmailConfirmado = false,
            TokenVerificacaoEmail = RandomNumberGenerator.GetHexString(48),
            TokenVerificacaoExpiraEm = DateTime.UtcNow.AddMinutes(MinutosExpiracaoVerificacao)
        };
        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, senha);

        var criado = await _usuarioRepository.AddAsync(usuario);

        await _clienteRepository.VincularUsuarioAsync(cliente.Id, criado.Id);

        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5500";
        var link = $"{baseUrl}/confirmar-email.html?token={criado.TokenVerificacaoEmail}";
        await _emailService.EnviarVerificacaoEmailAsync(email, nome, link);

        return criado;
    }

    public async Task<bool> ConfirmarEmailAsync(string token)
    {
        var usuario = await _usuarioRepository.GetByTokenVerificacaoAsync(token);
        if (usuario is null || usuario.TokenVerificacaoExpiraEm is null || usuario.TokenVerificacaoExpiraEm < DateTime.UtcNow)
            return false;

        usuario.EmailConfirmado = true;
        usuario.TokenVerificacaoEmail = null;
        usuario.TokenVerificacaoExpiraEm = null;

        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public async Task SolicitarRedefinicaoSenhaAsync(string email)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        if (usuario is null || usuario.Papel != PapelUsuario.Cliente) return;

        usuario.TokenResetSenha = RandomNumberGenerator.GetHexString(48);
        usuario.TokenResetSenhaExpiraEm = DateTime.UtcNow.AddMinutes(MinutosExpiracaoResetSenha);
        await _usuarioRepository.UpdateAsync(usuario);

        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5500";
        var link = $"{baseUrl}/redefinir-senha.html?token={usuario.TokenResetSenha}";
        await _emailService.EnviarRedefinicaoSenhaAsync(usuario.Email, usuario.Nome, link);
    }

    public async Task<bool> RedefinirSenhaAsync(string token, string novaSenha)
    {
        var usuario = await _usuarioRepository.GetByTokenResetSenhaAsync(token);
        if (usuario is null || usuario.TokenResetSenhaExpiraEm is null || usuario.TokenResetSenhaExpiraEm < DateTime.UtcNow)
            return false;

        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, novaSenha);
        usuario.TokenResetSenha = null;
        usuario.TokenResetSenhaExpiraEm = null;

        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public async Task GarantirContaVinculadaAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente is null || cliente.UsuarioId is not null || string.IsNullOrWhiteSpace(cliente.Email))
            return;

        var usuarioExistente = await _usuarioRepository.GetByEmailAsync(cliente.Email);
        if (usuarioExistente is not null)
        {
            if (usuarioExistente.Papel == PapelUsuario.Cliente)
                await _clienteRepository.VincularUsuarioAsync(cliente.Id, usuarioExistente.Id);
            return;
        }

        var usuario = new Usuario
        {
            Nome = cliente.Nome,
            Email = cliente.Email,
            Papel = PapelUsuario.Cliente,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            EmailConfirmado = true,
            TokenResetSenha = RandomNumberGenerator.GetHexString(48),
            TokenResetSenhaExpiraEm = DateTime.UtcNow.AddMinutes(MinutosExpiracaoResetSenha)
        };
        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, RandomNumberGenerator.GetHexString(32));

        var criado = await _usuarioRepository.AddAsync(usuario);
        await _clienteRepository.VincularUsuarioAsync(cliente.Id, criado.Id);

        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5500";
        var link = $"{baseUrl}/redefinir-senha.html?token={criado.TokenResetSenha}";
        await _emailService.EnviarBoasVindasContaAsync(cliente.Email, cliente.Nome, link);
    }
}
