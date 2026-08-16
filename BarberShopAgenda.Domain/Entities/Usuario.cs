using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public PapelUsuario Papel { get; set; } = PapelUsuario.Barbeiro;

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public bool EmailConfirmado { get; set; } = false;

    public string? TokenVerificacaoEmail { get; set; }
    public DateTime? TokenVerificacaoExpiraEm { get; set; }

    public string? TokenResetSenha { get; set; }
    public DateTime? TokenResetSenhaExpiraEm { get; set; }

    public Barbeiro? Barbeiro { get; set; }
    public Cliente? Cliente { get; set; }
}
