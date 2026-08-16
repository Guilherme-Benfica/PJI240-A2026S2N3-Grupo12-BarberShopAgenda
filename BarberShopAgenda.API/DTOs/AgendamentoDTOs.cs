using System.ComponentModel.DataAnnotations;
using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.API.DTOs;

public class AgendamentoCreateDTO
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int BarbeiroId { get; set; }

    [Required]
    public int ServicoId { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    [MaxLength(500)]
    public string? Observacao { get; set; }
}

public class AgendamentoResponseDTO
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int BarbeiroId { get; set; }
    public string BarbeiroNome { get; set; } = string.Empty;
    public int ServicoId { get; set; }
    public string ServicoNome { get; set; } = string.Empty;
    public decimal ServicoPreco { get; set; }
    public int ServicoDuracaoMinutos { get; set; }
    public DateTime DataHora { get; set; }
    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public string? Observacao { get; set; }
    public string CodigoConfirmacao { get; set; } = string.Empty;
}
