using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByTokenVerificacaoAsync(string token);
    Task<Usuario?> GetByTokenResetSenhaAsync(string token);
    Task<Usuario> AddAsync(Usuario usuario);
    Task<bool> UpdateAsync(Usuario usuario);
}
