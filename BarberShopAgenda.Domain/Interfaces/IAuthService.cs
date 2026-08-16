using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public class EmailNaoConfirmadoException : Exception
{
    public EmailNaoConfirmadoException(string message) : base(message)
    {
    }
}

public interface IAuthService
{
    Task<Usuario?> AutenticarAsync(string email, string senha);
    Task<bool> AlterarSenhaAsync(int usuarioId, string senhaAtual, string novaSenha);
}
