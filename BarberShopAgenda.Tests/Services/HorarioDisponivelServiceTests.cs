using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using BarberShopAgenda.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class HorarioDisponivelServiceTests
{
    private static (HorarioDisponivelService service, BarberShopContext context) CriarServicoComBanco(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        var context = new BarberShopContext(options);

        context.Barbeiros.Add(new Barbeiro
        {
            Id = 1,
            Nome = "Barbeiro Teste",
            Ativo = true,
            HorarioInicioManha = new TimeOnly(9, 0),
            HorarioFimManha = new TimeOnly(10, 0),
            HorarioInicioTarde = new TimeOnly(13, 0),
            HorarioFimTarde = new TimeOnly(14, 0),
            DiasTrabalho = 127 // todos os dias, para o teste não depender do dia da semana em que roda
        });
        context.Servicos.Add(new Servico { Id = 1, Nome = "Corte", Preco = 40m, DuracaoMinutos = 30 });
        context.Clientes.Add(new Cliente { Id = 1, Nome = "Cliente Teste", Telefone = "11999990000", DataCadastro = DateTime.UtcNow });
        context.SaveChanges();

        var barbeiroRepository = new BarbeiroRepository(context);
        var servicoRepository = new ServicoRepository(context);
        var agendamentoRepository = new AgendamentoRepository(context);

        var service = new HorarioDisponivelService(barbeiroRepository, servicoRepository, agendamentoRepository);
        return (service, context);
    }

    [Fact]
    public async Task ObterHorariosDisponiveisAsync_ComJanelasConfiguradas_DeveRetornarSlotsA15Minutos()
    {
        var (service, _) = CriarServicoComBanco(nameof(ObterHorariosDisponiveisAsync_ComJanelasConfiguradas_DeveRetornarSlotsA15Minutos));
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var horarios = (await service.ObterHorariosDisponiveisAsync(1, data, 1)).ToList();

        Assert.Contains(new TimeOnly(9, 0), horarios);
        Assert.Contains(new TimeOnly(9, 45), horarios);
        Assert.Contains(new TimeOnly(13, 0), horarios);
        Assert.DoesNotContain(new TimeOnly(10, 0), horarios); // fora da janela da manhã
        Assert.DoesNotContain(new TimeOnly(12, 0), horarios); // horário de almoço
    }

    [Fact]
    public async Task ObterHorariosDisponiveisAsync_ComHorarioOcupado_DeveExcluirSlotSobreposto()
    {
        var (service, context) = CriarServicoComBanco(nameof(ObterHorariosDisponiveisAsync_ComHorarioOcupado_DeveExcluirSlotSobreposto));
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        context.Agendamentos.Add(new Agendamento
        {
            ClienteId = 1,
            BarbeiroId = 1,
            ServicoId = 1,
            DataHora = data.ToDateTime(new TimeOnly(9, 0)),
            Status = StatusAgendamento.Pendente,
            CodigoConfirmacao = "ABC123"
        });
        context.SaveChanges();

        var horarios = (await service.ObterHorariosDisponiveisAsync(1, data, 1)).ToList();

        Assert.DoesNotContain(new TimeOnly(9, 0), horarios);
        Assert.Contains(new TimeOnly(9, 30), horarios);
    }

    [Fact]
    public async Task ObterHorariosDisponiveisAsync_ComDataNoPassado_NaoDeveRetornarHorariosJaPassados()
    {
        var (service, _) = CriarServicoComBanco(nameof(ObterHorariosDisponiveisAsync_ComDataNoPassado_NaoDeveRetornarHorariosJaPassados));
        var ontem = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var horarios = await service.ObterHorariosDisponiveisAsync(1, ontem, 1);

        Assert.Empty(horarios);
    }
}
