using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IBarbeiroRepository
{
    /// <summary>Todos os barbeiros, independente da situação da conta — uso administrativo.</summary>
    Task<IEnumerable<Barbeiro>> GetAllAsync();

    /// <summary>Só os barbeiros com conta ativa (ou sem conta vinculada) — o que fica visível pra clientes e demais usuários.</summary>
    Task<IEnumerable<Barbeiro>> GetAllVisiveisAsync();

    Task<Barbeiro?> GetByIdAsync(int id);
    Task<Barbeiro> AddAsync(Barbeiro barbeiro);
    Task<bool> UpdateAsync(Barbeiro barbeiro);
}
