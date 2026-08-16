using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
