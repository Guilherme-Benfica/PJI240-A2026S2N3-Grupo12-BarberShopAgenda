using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.API.DTOs;

public class ClienteCreateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [EmailAddress, MaxLength(150)]
    public string? Email { get; set; }
}

public class ClienteUpdateDTO : ClienteCreateDTO
{
}

public class ClienteResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DataCadastro { get; set; }
}
