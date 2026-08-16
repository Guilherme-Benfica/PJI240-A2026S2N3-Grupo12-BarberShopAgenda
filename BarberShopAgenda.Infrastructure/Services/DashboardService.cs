using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;

namespace BarberShopAgenda.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IAgendamentoRepository _agendamentoRepository;

    public DashboardService(IAgendamentoRepository agendamentoRepository)
    {
        _agendamentoRepository = agendamentoRepository;
    }

    public async Task<ResumoDoDia> GetResumoHojeAsync()
    {
        var agendamentosHoje = (await _agendamentoRepository.GetByDataAsync(DateTime.Today)).ToList();

        var pendentes = agendamentosHoje.Count(a => a.Status == StatusAgendamento.Pendente);
        var confirmados = agendamentosHoje.Count(a => a.Status == StatusAgendamento.Confirmado);
        var concluidos = agendamentosHoje.Count(a => a.Status == StatusAgendamento.Concluido);
        var cancelados = agendamentosHoje.Count(a => a.Status == StatusAgendamento.Cancelado);

        var receitaPrevista = agendamentosHoje
            .Where(a => a.Status != StatusAgendamento.Cancelado)
            .Sum(a => a.Servico?.Preco ?? 0);

        return new ResumoDoDia(
            TotalAgendamentos: agendamentosHoje.Count,
            Pendentes: pendentes,
            Confirmados: confirmados,
            Concluidos: concluidos,
            Cancelados: cancelados,
            ReceitaPrevista: receitaPrevista);
    }
}
