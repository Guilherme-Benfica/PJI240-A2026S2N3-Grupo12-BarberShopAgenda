using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public class ConflitoHorarioException : Exception
{
    public ConflitoHorarioException(string message) : base(message)
    {
    }
}

public class RegraNegocioException : Exception
{
    public RegraNegocioException(string message) : base(message)
    {
    }
}

public interface IAgendamentoService
{
    Task<IEnumerable<Agendamento>> GetAllAsync();
    Task<Agendamento?> GetByIdAsync(int id);
    Task<IEnumerable<Agendamento>> GetByBarbeiroAsync(int barbeiroId);
    Task<IEnumerable<Agendamento>> GetByClienteAsync(int clienteId);
    Task<IEnumerable<Agendamento>> GetByDataAsync(DateTime data);
    Task<Agendamento> CriarAsync(int clienteId, int barbeiroId, int servicoId, DateTime dataHora, string? observacao);
    Task<bool> ConfirmarAsync(int id);
    Task<bool> CancelarAsync(int id);
    Task<bool> ConcluirAsync(int id);
}
