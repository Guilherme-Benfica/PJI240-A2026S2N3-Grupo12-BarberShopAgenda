namespace BarberShopAgenda.Domain.Interfaces;

public interface IHorarioDisponivelService
{
    Task<IEnumerable<TimeOnly>> ObterHorariosDisponiveisAsync(int barbeiroId, DateOnly data, int servicoId);
}
