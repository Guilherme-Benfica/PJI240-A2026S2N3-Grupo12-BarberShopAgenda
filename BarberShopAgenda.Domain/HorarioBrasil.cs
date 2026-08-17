namespace BarberShopAgenda.Domain;

/// <summary>
/// Horário "de agora"/"de hoje" no fuso do Brasil, independente do fuso do servidor.
/// Necessário porque em produção o container roda em UTC, mas todo DataHora de
/// agendamento é tratado como hora local do Brasil (sem timezone) — usar
/// DateTime.Now/Today direto deixa esses cálculos ~3h errados em produção.
/// </summary>
public static class HorarioBrasil
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime Agora => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso);

    public static DateTime Hoje => Agora.Date;
}
