using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.API.DTOs;

public class ServicoCreateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Range(0, 100000)]
    public decimal Preco { get; set; }

    [Range(1, 600)]
    public int DuracaoMinutos { get; set; }
}

public class ServicoUpdateDTO : ServicoCreateDTO
{
}

public class ServicoResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int DuracaoMinutos { get; set; }
}
