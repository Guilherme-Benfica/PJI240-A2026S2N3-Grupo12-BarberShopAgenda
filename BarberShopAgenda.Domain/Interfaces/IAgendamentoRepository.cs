using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IAgendamentoRepository
{
    Task<IEnumerable<Agendamento>> GetAllAsync();
    Task<Agendamento?> GetByIdAsync(int id);
    Task<IEnumerable<Agendamento>> GetByBarbeiroAsync(int barbeiroId);
    Task<IEnumerable<Agendamento>> GetByClienteAsync(int clienteId);
    Task<IEnumerable<Agendamento>> GetByDataAsync(DateTime data);
    Task<bool> ExisteConflitoAsync(int barbeiroId, DateTime dataHora, int duracaoMinutos, int? agendamentoIdIgnorar = null);
    Task<bool> ExisteConflitoClienteAsync(int clienteId, DateTime dataHora, int duracaoMinutos, int? agendamentoIdIgnorar = null);
    Task<Agendamento> AddAsync(Agendamento agendamento);
    Task<bool> UpdateAsync(Agendamento agendamento);
}
