using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.API.DTOs;

public class AuthLoginDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
    public int? BarbeiroId { get; set; }
    public int? ClienteId { get; set; }
}

public class RegistrarClienteDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}

public class ConfirmarEmailDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class EsqueciSenhaDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class RedefinirSenhaDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NovaSenha { get; set; } = string.Empty;
}

public class AlterarSenhaDTO
{
    [Required]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NovaSenha { get; set; } = string.Empty;
}
