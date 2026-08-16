using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;

namespace BarberShopAgenda.Infrastructure.Services;

public class HorarioDisponivelService : IHorarioDisponivelService
{
    private const int IntervaloMinutos = 15;

    private readonly IBarbeiroRepository _barbeiroRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;

    public HorarioDisponivelService(
        IBarbeiroRepository barbeiroRepository,
        IServicoRepository servicoRepository,
        IAgendamentoRepository agendamentoRepository)
    {
        _barbeiroRepository = barbeiroRepository;
        _servicoRepository = servicoRepository;
        _agendamentoRepository = agendamentoRepository;
    }

    public async Task<IEnumerable<TimeOnly>> ObterHorariosDisponiveisAsync(int barbeiroId, DateOnly data, int servicoId)
    {
        if (data < DateOnly.FromDateTime(DateTime.Today)) return [];

        var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);
        if (barbeiro is null || !barbeiro.Ativo) return [];
        if (barbeiro.Usuario is not null && !barbeiro.Usuario.Ativo) return [];
        if (EstaDeFerias(barbeiro, data)) return [];

        var servico = await _servicoRepository.GetByIdAsync(servicoId);
        if (servico is null) return [];

        var diaBit = DiaParaBit(data.DayOfWeek);
        if ((barbeiro.DiasTrabalho & diaBit) == 0) return [];

        var candidatos = GerarCandidatos(barbeiro);
        if (candidatos.Count == 0) return [];

        var agendamentosDoDia = (await _agendamentoRepository.GetByBarbeiroAsync(barbeiroId))
            .Where(a => a.Status != StatusAgendamento.Cancelado && DateOnly.FromDateTime(a.DataHora) == data)
            .ToList();

        var agora = DateTime.Now;

        return candidatos.Where(horario =>
        {
            var inicio = data.ToDateTime(horario);
            var fim = inicio.AddMinutes(servico.DuracaoMinutos);

            if (data == DateOnly.FromDateTime(DateTime.Today) && inicio < agora) return false;

            return !agendamentosDoDia.Any(a =>
            {
                var existenteInicio = a.DataHora;
                var existenteFim = a.DataHora.AddMinutes(a.Servico?.DuracaoMinutos ?? 0);
                return existenteInicio < fim && inicio < existenteFim;
            });
        });
    }

    private static bool EstaDeFerias(Barbeiro barbeiro, DateOnly data)
        => barbeiro.FeriasInicio is not null && barbeiro.FeriasFim is not null
           && data >= barbeiro.FeriasInicio.Value && data <= barbeiro.FeriasFim.Value;

    private static List<TimeOnly> GerarCandidatos(Barbeiro barbeiro)
    {
        var candidatos = new List<TimeOnly>();
        AdicionarJanela(candidatos, barbeiro.HorarioInicioManha, barbeiro.HorarioFimManha);
        AdicionarJanela(candidatos, barbeiro.HorarioInicioTarde, barbeiro.HorarioFimTarde);
        return candidatos;
    }

    private static void AdicionarJanela(List<TimeOnly> candidatos, TimeOnly? inicio, TimeOnly? fim)
    {
        if (inicio is null || fim is null || inicio >= fim) return;

        var atual = inicio.Value;
        while (atual < fim.Value)
        {
            candidatos.Add(atual);
            atual = atual.AddMinutes(IntervaloMinutos);
        }
    }

    private static byte DiaParaBit(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 4,
        DayOfWeek.Thursday => 8,
        DayOfWeek.Friday => 16,
        DayOfWeek.Saturday => 32,
        DayOfWeek.Sunday => 64,
        _ => 0
    };
}
