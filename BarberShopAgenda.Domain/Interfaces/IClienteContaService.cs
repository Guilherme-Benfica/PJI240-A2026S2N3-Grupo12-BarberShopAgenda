using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IClienteContaService
{
    Task<Usuario> RegistrarAsync(string nome, string telefone, string email, string senha);
    Task<bool> ConfirmarEmailAsync(string token);
    Task SolicitarRedefinicaoSenhaAsync(string email);
    Task<bool> RedefinirSenhaAsync(string token, string novaSenha);
}
