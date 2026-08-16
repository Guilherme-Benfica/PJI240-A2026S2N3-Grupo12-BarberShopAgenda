namespace BarberShopAgenda.Domain.Interfaces;

public interface IEmailService
{
    Task EnviarConfirmacaoAgendamentoAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string servicoNome,
        string barbeiroNome,
        DateTime dataHora,
        string codigoConfirmacao);

    Task EnviarVerificacaoEmailAsync(string destinatarioEmail, string destinatarioNome, string linkVerificacao);

    Task EnviarRedefinicaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRedefinicao);
}
