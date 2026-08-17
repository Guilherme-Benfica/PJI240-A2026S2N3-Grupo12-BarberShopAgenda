using System.Net.Http.Headers;
using System.Net.Http.Json;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BarberShopAgenda.Infrastructure.Services;

/// <summary>
/// Envia e-mails pela API HTTP transacional da Brevo (porta 443) em vez de SMTP —
/// hosts como o Render bloqueiam as portas SMTP (25/465/587) na camada de rede do plano gratuito,
/// o que fazia toda conexão SMTP travar por ~100s até estourar timeout e falhar.
/// </summary>
public class BrevoEmailService : IEmailService
{
    private const string EndpointEnvioEmail = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
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
        var apiKey = Environment.GetEnvironmentVariable("BARBERSHOP_BREVO_API_KEY") ?? _configuration["Brevo:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("Brevo não configurado — e-mail \"{Assunto}\" não enviado para {Email}.", assunto, destinatarioEmail);
            return;
        }

        var remetenteEmail = _configuration["Email:RemetenteEmail"] ?? "barbershopagenda90@gmail.com";
        var remetenteNome = _configuration["Email:RemetenteNome"] ?? "BarberShop Agenda";

        var payload = new
        {
            sender = new { name = remetenteNome, email = remetenteEmail },
            to = new[] { new { email = destinatarioEmail, name = destinatarioNome } },
            subject = assunto,
            htmlContent = corpoHtml
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, EndpointEnvioEmail)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var resposta = await _httpClient.SendAsync(request);
            if (!resposta.IsSuccessStatusCode)
            {
                var corpoResposta = await resposta.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Falha ao enviar e-mail \"{Assunto}\" para {Email}. Status {Status}: {Corpo}",
                    assunto, destinatarioEmail, resposta.StatusCode, corpoResposta);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail \"{Assunto}\" para {Email}.", assunto, destinatarioEmail);
        }
    }
}
