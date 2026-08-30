using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BarberShopAgenda.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        if (usuario is null || !usuario.Ativo) return null;

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (resultado == PasswordVerificationResult.Failed) return null;

        if (usuario.Papel == PapelUsuario.Cliente && !usuario.EmailConfirmado)
            throw new EmailNaoConfirmadoException("Confirme seu e-mail antes de entrar. Verifique sua caixa de entrada.");

        return usuario;
    }

    public async Task<bool> AlterarSenhaAsync(int usuarioId, string senhaAtual, string novaSenha)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario is null) return false;

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senhaAtual);
        if (resultado == PasswordVerificationResult.Failed) return false;

        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, novaSenha);
        return await _usuarioRepository.UpdateAsync(usuario);
    }
}
