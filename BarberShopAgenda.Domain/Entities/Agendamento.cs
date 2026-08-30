using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarberShopAgenda.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    public int BarbeiroId { get; set; }
    public Barbeiro? Barbeiro { get; set; }

    [Required]
    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;

    [MaxLength(500)]
    public string? Observacao { get; set; }

    [Required, MaxLength(6)]
    public string CodigoConfirmacao { get; set; } = string.Empty;
}
