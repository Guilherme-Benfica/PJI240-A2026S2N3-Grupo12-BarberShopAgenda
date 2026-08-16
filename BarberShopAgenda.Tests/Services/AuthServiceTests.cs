using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using BarberShopAgenda.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class AuthServiceTests
{
    private static (AuthService service, Usuario usuario) CriarServicoComUsuario(
        string nomeBanco, bool ativo = true, PapelUsuario papel = PapelUsuario.Admin, bool emailConfirmado = true)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        var context = new BarberShopContext(options);
        var hasher = new PasswordHasher<Usuario>();

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Admin Teste",
            Email = "admin@teste.com",
            Papel = papel,
            Ativo = ativo,
            EmailConfirmado = emailConfirmado,
            DataCadastro = DateTime.UtcNow
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, "Senha@123");

        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var repository = new UsuarioRepository(context);
        return (new AuthService(repository), usuario);
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaCorreta_DeveRetornarUsuario()
    {
        var (service, usuario) = CriarServicoComUsuario(nameof(AutenticarAsync_ComSenhaCorreta_DeveRetornarUsuario));

        var resultado = await service.AutenticarAsync(usuario.Email, "Senha@123");

        Assert.NotNull(resultado);
        Assert.Equal(usuario.Email, resultado!.Email);
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaIncorreta_DeveRetornarNulo()
    {
        var (service, usuario) = CriarServicoComUsuario(nameof(AutenticarAsync_ComSenhaIncorreta_DeveRetornarNulo));

        var resultado = await service.AutenticarAsync(usuario.Email, "senha-errada");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_ComUsuarioInativo_DeveRetornarNulo()
    {
        var (service, usuario) = CriarServicoComUsuario(nameof(AutenticarAsync_ComUsuarioInativo_DeveRetornarNulo), ativo: false);

        var resultado = await service.AutenticarAsync(usuario.Email, "Senha@123");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_ComClienteSemEmailConfirmado_DeveLancarEmailNaoConfirmadoException()
    {
        var (service, usuario) = CriarServicoComUsuario(
            nameof(AutenticarAsync_ComClienteSemEmailConfirmado_DeveLancarEmailNaoConfirmadoException),
            papel: PapelUsuario.Cliente, emailConfirmado: false);

        await Assert.ThrowsAsync<EmailNaoConfirmadoException>(
            () => service.AutenticarAsync(usuario.Email, "Senha@123"));
    }

    [Fact]
    public async Task AlterarSenhaAsync_ComSenhaAtualCorreta_DeveTrocarSenha()
    {
        var (service, usuario) = CriarServicoComUsuario(nameof(AlterarSenhaAsync_ComSenhaAtualCorreta_DeveTrocarSenha));

        var alterada = await service.AlterarSenhaAsync(usuario.Id, "Senha@123", "SenhaNova@456");
        Assert.True(alterada);

        var loginComSenhaAntiga = await service.AutenticarAsync(usuario.Email, "Senha@123");
        var loginComSenhaNova = await service.AutenticarAsync(usuario.Email, "SenhaNova@456");
        Assert.Null(loginComSenhaAntiga);
        Assert.NotNull(loginComSenhaNova);
    }

    [Fact]
    public async Task AlterarSenhaAsync_ComSenhaAtualIncorreta_DeveFalhar()
    {
        var (service, usuario) = CriarServicoComUsuario(nameof(AlterarSenhaAsync_ComSenhaAtualIncorreta_DeveFalhar));

        var alterada = await service.AlterarSenhaAsync(usuario.Id, "senha-errada", "SenhaNova@456");

        Assert.False(alterada);
    }
}
