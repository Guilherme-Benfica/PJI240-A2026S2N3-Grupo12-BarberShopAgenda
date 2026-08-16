using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using BarberShopAgenda.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class AgendamentoServiceTests
{
    private static (IAgendamentoService service, BarberShopContext context, Mock<IEmailService> emailServiceMock) CriarServicoComBanco(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        var context = new BarberShopContext(options);

        context.Clientes.Add(new Cliente { Id = 1, Nome = "Cliente Teste", Telefone = "11999990000", Email = "cliente@teste.com", DataCadastro = DateTime.UtcNow });
        context.Barbeiros.Add(new Barbeiro { Id = 1, Nome = "Barbeiro Teste", Ativo = true });
        context.Servicos.Add(new Servico { Id = 1, Nome = "Corte", Preco = 40m, DuracaoMinutos = 30 });
        context.SaveChanges();

        var agendamentoRepository = new AgendamentoRepository(context);
        var clienteRepository = new ClienteRepository(context);
        var barbeiroRepository = new BarbeiroRepository(context);
        var servicoRepository = new ServicoRepository(context);
        var emailServiceMock = new Mock<IEmailService>();

        var service = new AgendamentoService(
            agendamentoRepository, clienteRepository, barbeiroRepository, servicoRepository,
            emailServiceMock.Object, NullLogger<AgendamentoService>.Instance);

        return (service, context, emailServiceMock);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCriarAgendamentoPendente()
    {
        var (service, _, _) = CriarServicoComBanco(nameof(CriarAsync_ComDadosValidos_DeveCriarAgendamentoPendente));
        var dataHora = DateTime.Today.AddDays(1).AddHours(10);

        var agendamento = await service.CriarAsync(1, 1, 1, dataHora, "Primeira vez");

        Assert.NotEqual(0, agendamento.Id);
        Assert.Equal(StatusAgendamento.Pendente, agendamento.Status);
        Assert.Equal(dataHora, agendamento.DataHora);
        Assert.Equal(6, agendamento.CodigoConfirmacao.Length);
    }

    [Fact]
    public async Task CriarAsync_ComClienteComEmail_DeveEnviarEmailDeConfirmacao()
    {
        var (service, _, emailServiceMock) = CriarServicoComBanco(nameof(CriarAsync_ComClienteComEmail_DeveEnviarEmailDeConfirmacao));
        var dataHora = DateTime.Today.AddDays(1).AddHours(11);

        var agendamento = await service.CriarAsync(1, 1, 1, dataHora, null);

        emailServiceMock.Verify(e => e.EnviarConfirmacaoAgendamentoAsync(
            "cliente@teste.com", "Cliente Teste", "Corte", "Barbeiro Teste", dataHora, agendamento.CodigoConfirmacao), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_QuandoEnvioDeEmailFalha_NaoDeveInterromperCriacaoDoAgendamento()
    {
        var (service, _, emailServiceMock) = CriarServicoComBanco(nameof(CriarAsync_QuandoEnvioDeEmailFalha_NaoDeveInterromperCriacaoDoAgendamento));
        emailServiceMock
            .Setup(e => e.EnviarConfirmacaoAgendamentoAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Falha simulada de SMTP"));

        var dataHora = DateTime.Today.AddDays(1).AddHours(12);
        var agendamento = await service.CriarAsync(1, 1, 1, dataHora, null);

        Assert.NotEqual(0, agendamento.Id);
        Assert.Equal(StatusAgendamento.Pendente, agendamento.Status);
    }

    [Fact]
    public async Task CriarAsync_ComConflitoDeHorario_DeveLancarConflitoHorarioException()
    {
        var (service, _, _) = CriarServicoComBanco(nameof(CriarAsync_ComConflitoDeHorario_DeveLancarConflitoHorarioException));
        var dataHora = DateTime.Today.AddDays(1).AddHours(14);

        await service.CriarAsync(1, 1, 1, dataHora, null);

        await Assert.ThrowsAsync<ConflitoHorarioException>(
            () => service.CriarAsync(1, 1, 1, dataHora.AddMinutes(10), null));
    }

    [Fact]
    public async Task CriarAsync_ComClienteJaAgendadoNoMesmoHorarioComOutroBarbeiro_DeveLancarConflitoHorarioException()
    {
        var (service, context, _) = CriarServicoComBanco(nameof(CriarAsync_ComClienteJaAgendadoNoMesmoHorarioComOutroBarbeiro_DeveLancarConflitoHorarioException));
        context.Barbeiros.Add(new Barbeiro { Id = 2, Nome = "Outro Barbeiro", Ativo = true });
        context.SaveChanges();

        var dataHora = DateTime.Today.AddDays(1).AddHours(10);
        await service.CriarAsync(1, 1, 1, dataHora, null);

        await Assert.ThrowsAsync<ConflitoHorarioException>(
            () => service.CriarAsync(1, 2, 1, dataHora.AddMinutes(10), null));
    }

    [Fact]
    public async Task CancelarAsync_ComAgendamentoExistente_DeveAlterarStatusParaCancelado()
    {
        var (service, _, _) = CriarServicoComBanco(nameof(CancelarAsync_ComAgendamentoExistente_DeveAlterarStatusParaCancelado));
        var criado = await service.CriarAsync(1, 1, 1, DateTime.Today.AddDays(1).AddHours(9), null);

        var cancelado = await service.CancelarAsync(criado.Id);
        var agendamento = await service.GetByIdAsync(criado.Id);

        Assert.True(cancelado);
        Assert.Equal(StatusAgendamento.Cancelado, agendamento!.Status);
    }

    [Fact]
    public async Task GetByDataAsync_DeveRetornarApenasAgendamentosDaDataInformada()
    {
        var (service, _, _) = CriarServicoComBanco(nameof(GetByDataAsync_DeveRetornarApenasAgendamentosDaDataInformada));
        var hoje = DateTime.Today.AddDays(1);
        var amanha = hoje.AddDays(1);

        await service.CriarAsync(1, 1, 1, hoje.AddHours(9), null);
        await service.CriarAsync(1, 1, 1, amanha.AddHours(9), null);

        var agendamentosDeHoje = await service.GetByDataAsync(hoje);

        Assert.Single(agendamentosDeHoje);
        Assert.Equal(hoje.AddHours(9), agendamentosDeHoje.First().DataHora);
    }
}
