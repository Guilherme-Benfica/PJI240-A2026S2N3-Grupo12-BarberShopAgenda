using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IServicoRepository
{
    Task<IEnumerable<Servico>> GetAllAsync();
    Task<Servico?> GetByIdAsync(int id);
    Task<Servico> AddAsync(Servico servico);
    Task<bool> UpdateAsync(Servico servico);
}
