using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using BarberShopAgenda.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class ClienteContaServiceTests
{
    private static (IClienteContaService service, BarberShopContext context, IUsuarioRepository usuarioRepository) CriarServico(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        var context = new BarberShopContext(options);
        var usuarioRepository = new UsuarioRepository(context);
        var clienteRepository = new ClienteRepository(context);
        var configuration = new ConfigurationBuilder().Build();
        var emailServiceMock = new Mock<IEmailService>();

        var service = new ClienteContaService(usuarioRepository, clienteRepository, emailServiceMock.Object, configuration);
        return (service, context, usuarioRepository);
    }

    [Fact]
    public async Task RegistrarAsync_SemAgendamentoAnterior_DeveCriarUsuarioEClienteVinculados()
    {
        var (service, context, _) = CriarServico(nameof(RegistrarAsync_SemAgendamentoAnterior_DeveCriarUsuarioEClienteVinculados));

        var usuario = await service.RegistrarAsync("Novo Cliente", "11988887777", "novo@teste.com", "Senha@123");

        Assert.False(usuario.EmailConfirmado);
        Assert.NotNull(usuario.TokenVerificacaoEmail);

        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Telefone == "11988887777");
        Assert.NotNull(cliente);
        Assert.Equal(usuario.Id, cliente!.UsuarioId);
    }

    [Fact]
    public async Task RegistrarAsync_ComClienteConvidadoExistente_DeveVincularAoInvesDeDuplicar()
    {
        var (service, context, _) = CriarServico(nameof(RegistrarAsync_ComClienteConvidadoExistente_DeveVincularAoInvesDeDuplicar));
        context.Clientes.Add(new Cliente { Id = 10, Nome = "Cliente Convidado", Telefone = "11977776666", DataCadastro = DateTime.UtcNow });
        context.SaveChanges();

        await service.RegistrarAsync("Cliente Convidado", "11977776666", "convidado@teste.com", "Senha@123");

        var totalClientesComEsseTelefone = await context.Clientes.CountAsync(c => c.Telefone == "11977776666");
        Assert.Equal(1, totalClientesComEsseTelefone);

        var cliente = await context.Clientes.FirstAsync(c => c.Telefone == "11977776666");
        Assert.NotNull(cliente.UsuarioId);
    }

    [Fact]
    public async Task ConfirmarEmailAsync_ComTokenValido_DeveMarcarEmailConfirmado()
    {
        var (service, _, usuarioRepository) = CriarServico(nameof(ConfirmarEmailAsync_ComTokenValido_DeveMarcarEmailConfirmado));
        var usuario = await service.RegistrarAsync("Cliente Teste", "11966665555", "confirmar@teste.com", "Senha@123");

        var confirmado = await service.ConfirmarEmailAsync(usuario.TokenVerificacaoEmail!);

        Assert.True(confirmado);
        var atualizado = await usuarioRepository.GetByIdAsync(usuario.Id);
        Assert.True(atualizado!.EmailConfirmado);
        Assert.Null(atualizado.TokenVerificacaoEmail);
    }

    [Fact]
    public async Task ConfirmarEmailAsync_ComTokenExpirado_DeveFalhar()
    {
        var (service, context, _) = CriarServico(nameof(ConfirmarEmailAsync_ComTokenExpirado_DeveFalhar));
        var usuario = await service.RegistrarAsync("Cliente Teste", "11955554444", "expirado@teste.com", "Senha@123");

        var entidade = await context.Usuarios.FindAsync(usuario.Id);
        entidade!.TokenVerificacaoExpiraEm = DateTime.UtcNow.AddMinutes(-10);
        await context.SaveChangesAsync();

        var confirmado = await service.ConfirmarEmailAsync(usuario.TokenVerificacaoEmail!);

        Assert.False(confirmado);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_ComTokenValido_DeveTrocarSenha()
    {
        var (service, context, usuarioRepository) = CriarServico(nameof(RedefinirSenhaAsync_ComTokenValido_DeveTrocarSenha));
        var usuario = await service.RegistrarAsync("Cliente Teste", "11944443333", "redefinir@teste.com", "SenhaAntiga@123");
        var hashAntigo = usuario.SenhaHash;

        await service.SolicitarRedefinicaoSenhaAsync("redefinir@teste.com");
        var entidade = await context.Usuarios.AsNoTracking().FirstAsync(u => u.Id == usuario.Id);

        var redefinida = await service.RedefinirSenhaAsync(entidade.TokenResetSenha!, "SenhaNova@123");

        Assert.True(redefinida);
        var atualizado = await usuarioRepository.GetByIdAsync(usuario.Id);
        Assert.NotEqual(hashAntigo, atualizado!.SenhaHash);
        Assert.Null(atualizado.TokenResetSenha);
    }
}
