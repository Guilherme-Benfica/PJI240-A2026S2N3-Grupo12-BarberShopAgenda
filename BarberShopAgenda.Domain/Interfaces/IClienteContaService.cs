using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface IClienteContaService
{
    Task<Usuario> RegistrarAsync(string nome, string telefone, string email, string senha);
    Task<bool> ConfirmarEmailAsync(string token);
    Task SolicitarRedefinicaoSenhaAsync(string email);
    Task<bool> RedefinirSenhaAsync(string token, string novaSenha);

    /// <summary>
    /// Cria automaticamente uma conta de login (já com e-mail confirmado) para um cliente que
    /// acabou de agendar sem ter conta ainda, e envia um link para ele definir a própria senha.
    /// Não faz nada se o cliente já tiver conta vinculada, não tiver e-mail, ou se o e-mail já
    /// pertencer a uma conta de outro papel.
    /// </summary>
    Task GarantirContaVinculadaAsync(int clienteId);
}
