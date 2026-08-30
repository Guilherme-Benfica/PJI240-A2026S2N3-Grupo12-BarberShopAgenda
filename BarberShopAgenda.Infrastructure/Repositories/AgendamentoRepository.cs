using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly BarberShopContext _context;

    public AgendamentoRepository(BarberShopContext context)
    {
        _context = context;
    }

    private IQueryable<Agendamento> QueryComIncludes()
        => _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Barbeiro)
            .Include(a => a.Servico);

    public async Task<IEnumerable<Agendamento>> GetAllAsync()
        => await QueryComIncludes().AsNoTracking().OrderBy(a => a.DataHora).ToListAsync();

    public async Task<Agendamento?> GetByIdAsync(int id)
        => await QueryComIncludes().AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Agendamento>> GetByBarbeiroAsync(int barbeiroId)
        => await QueryComIncludes().AsNoTracking()
            .Where(a => a.BarbeiroId == barbeiroId)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Agendamento>> GetByClienteAsync(int clienteId)
        => await QueryComIncludes().AsNoTracking()
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Agendamento>> GetByDataAsync(DateTime data)
    {
        var inicio = data.Date;
        var fim = inicio.AddDays(1);

        return await QueryComIncludes().AsNoTracking()
            .Where(a => a.DataHora >= inicio && a.DataHora < fim)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public Task<bool> ExisteConflitoAsync(int barbeiroId, DateTime dataHora, int duracaoMinutos, int? agendamentoIdIgnorar = null)
        => ExisteConflitoAsync(a => a.BarbeiroId == barbeiroId, dataHora, duracaoMinutos, agendamentoIdIgnorar);

    public Task<bool> ExisteConflitoClienteAsync(int clienteId, DateTime dataHora, int duracaoMinutos, int? agendamentoIdIgnorar = null)
        => ExisteConflitoAsync(a => a.ClienteId == clienteId, dataHora, duracaoMinutos, agendamentoIdIgnorar);

    private async Task<bool> ExisteConflitoAsync(
        System.Linq.Expressions.Expression<Func<Agendamento, bool>> filtro,
        DateTime dataHora,
        int duracaoMinutos,
        int? agendamentoIdIgnorar)
    {
        var novoInicio = dataHora;
        var novoFim = dataHora.AddMinutes(duracaoMinutos);

        var candidatos = await _context.Agendamentos
            .Include(a => a.Servico)
            .Where(filtro)
            .Where(a => a.Status != StatusAgendamento.Cancelado
                        && (agendamentoIdIgnorar == null || a.Id != agendamentoIdIgnorar)
                        && a.DataHora < novoFim
                        && a.DataHora >= novoInicio.AddHours(-6))
            .AsNoTracking()
            .ToListAsync();

        return candidatos.Any(a =>
        {
            var existenteInicio = a.DataHora;
            var existenteFim = a.DataHora.AddMinutes(a.Servico?.DuracaoMinutos ?? 0);
            return existenteInicio < novoFim && novoInicio < existenteFim;
        });
    }

    public async Task<Agendamento> AddAsync(Agendamento agendamento)
    {
        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();
        return agendamento;
    }

    public async Task<bool> UpdateAsync(Agendamento agendamento)
    {
        var existente = await _context.Agendamentos.FindAsync(agendamento.Id);
        if (existente is null) return false;

        existente.ClienteId = agendamento.ClienteId;
        existente.BarbeiroId = agendamento.BarbeiroId;
        existente.ServicoId = agendamento.ServicoId;
        existente.DataHora = agendamento.DataHora;
        existente.Status = agendamento.Status;
        existente.Observacao = agendamento.Observacao;

        await _context.SaveChangesAsync();
        return true;
    }
}
