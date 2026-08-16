using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> GetAllAsync();
    Task<Cliente?> GetByIdAsync(int id);
    Task<Cliente?> GetByTelefoneAsync(string telefone);
    Task<Cliente> AddAsync(Cliente cliente);
    Task<bool> UpdateAsync(Cliente cliente);
    Task<bool> VincularUsuarioAsync(int clienteId, int usuarioId);
    Task<bool> DeleteAsync(int id);
}
