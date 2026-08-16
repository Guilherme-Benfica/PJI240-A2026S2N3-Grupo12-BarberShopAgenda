using BarberShopAgenda.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BarberShopAgenda.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task EnviarConfirmacaoAgendamentoAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string servicoNome,
        string barbeiroNome,
        DateTime dataHora,
        string codigoConfirmacao)
    {
        var corpo = $"""
            <div style="font-family: Arial, sans-serif; background:#0d0d0d; color:#f2f2f2; padding:24px;">
              <h1 style="color:#d4af37; font-size:20px;">Agendamento confirmado</h1>
              <p>Olá, {destinatarioNome}!</p>
              <p>Seu agendamento foi registrado com sucesso:</p>
              <ul>
                <li><strong>Serviço:</strong> {servicoNome}</li>
                <li><strong>Profissional:</strong> {barbeiroNome}</li>
                <li><strong>Data/hora:</strong> {dataHora:dd/MM/yyyy 'às' HH:mm}</li>
              </ul>
              <p>Guarde o código abaixo — ele também serve para consultar seus agendamentos depois:</p>
              <p style="font-size:24px; font-weight:bold; letter-spacing:4px; color:#d4af37; background:#1a1a1a; padding:12px; text-align:center; border-radius:8px;">{codigoConfirmacao}</p>
              <p style="color:#b8b8b8; font-size:13px;">BarberShop Agenda — sistema de agendamento para barbearias</p>
            </div>
            """;

        return EnviarAsync(destinatarioEmail, destinatarioNome, "Agendamento confirmado — BarberShop Agenda", corpo);
    }

    public Task EnviarVerificacaoEmailAsync(string destinatarioEmail, string destinatarioNome, string linkVerificacao)
    {
        var corpo = $"""
            <div style="font-family: Arial, sans-serif; background:#0d0d0d; color:#f2f2f2; padding:24px;">
              <h1 style="color:#d4af37; font-size:20px;">Confirme seu e-mail</h1>
              <p>Olá, {destinatarioNome}!</p>
              <p>Falta pouco para ativar sua conta no BarberShop Agenda. Clique no botão abaixo para confirmar seu e-mail:</p>
              <p style="text-align:center; margin:24px 0;">
                <a href="{linkVerificacao}" style="background:#d4af37; color:#000; text-decoration:none; font-weight:bold; padding:12px 24px; border-radius:8px; display:inline-block;">Confirmar e-mail</a>
              </p>
              <p style="color:#b8b8b8; font-size:13px;">Se você não criou essa conta, pode ignorar este e-mail. O link expira em 24 horas.</p>
              <p style="color:#b8b8b8; font-size:13px;">BarberShop Agenda — sistema de agendamento para barbearias</p>
            </div>
            """;

        return EnviarAsync(destinatarioEmail, destinatarioNome, "Confirme seu e-mail — BarberShop Agenda", corpo);
    }

    public Task EnviarRedefinicaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRedefinicao)
    {
        var corpo = $"""
            <div style="font-family: Arial, sans-serif; background:#0d0d0d; color:#f2f2f2; padding:24px;">
              <h1 style="color:#d4af37; font-size:20px;">Redefinir senha</h1>
              <p>Olá, {destinatarioNome}!</p>
              <p>Recebemos um pedido para redefinir sua senha. Clique no botão abaixo para escolher uma nova:</p>
              <p style="text-align:center; margin:24px 0;">
                <a href="{linkRedefinicao}" style="background:#d4af37; color:#000; text-decoration:none; font-weight:bold; padding:12px 24px; border-radius:8px; display:inline-block;">Redefinir senha</a>
              </p>
              <p style="color:#b8b8b8; font-size:13px;">Se você não pediu isso, pode ignorar este e-mail — sua senha continua a mesma. O link expira em 1 hora.</p>
              <p style="color:#b8b8b8; font-size:13px;">BarberShop Agenda — sistema de agendamento para barbearias</p>
            </div>
            """;

        return EnviarAsync(destinatarioEmail, destinatarioNome, "Redefinir sua senha — BarberShop Agenda", corpo);
    }

    private async Task EnviarAsync(string destinatarioEmail, string destinatarioNome, string assunto, string corpoHtml)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("SMTP não configurado — e-mail \"{Assunto}\" não enviado para {Email}.", assunto, destinatarioEmail);
            return;
        }

        var porta = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var usuario = _configuration["Smtp:Usuario"];
        var senha = Environment.GetEnvironmentVariable("BARBERSHOP_SMTP_SENHA") ?? _configuration["Smtp:Senha"];
        var remetenteEmail = _configuration["Smtp:RemetenteEmail"] ?? usuario;
        var remetenteNome = _configuration["Smtp:RemetenteNome"] ?? "BarberShop Agenda";

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(remetenteNome, remetenteEmail));
        mensagem.To.Add(new MailboxAddress(destinatarioNome, destinatarioEmail));
        mensagem.Subject = assunto;
        mensagem.Body = new TextPart("html") { Text = corpoHtml };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, porta, SecureSocketOptions.StartTls);
            if (!string.IsNullOrWhiteSpace(usuario))
                await client.AuthenticateAsync(usuario, senha);

            await client.SendAsync(mensagem);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail \"{Assunto}\" para {Email}.", assunto, destinatarioEmail);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }
}
