namespace BarberShopAgenda.Domain.Interfaces;

public record ResumoDoDia(int TotalAgendamentos, int Pendentes, int Confirmados, int Concluidos, int Cancelados, decimal ReceitaPrevista);

public interface IDashboardService
{
    Task<ResumoDoDia> GetResumoHojeAsync();
}
