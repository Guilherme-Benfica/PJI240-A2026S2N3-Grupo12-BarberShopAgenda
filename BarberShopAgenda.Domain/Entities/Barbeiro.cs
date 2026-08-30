using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.Domain.Entities;

public class Barbeiro
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Especialidade { get; set; }

    public bool Ativo { get; set; } = true;

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public TimeOnly? HorarioInicioManha { get; set; }
    public TimeOnly? HorarioFimManha { get; set; }
    public TimeOnly? HorarioInicioTarde { get; set; }
    public TimeOnly? HorarioFimTarde { get; set; }

    /// <summary>Bitmask dos dias de trabalho: segunda=1, terça=2, quarta=4, quinta=8, sexta=16, sábado=32, domingo=64.</summary>
    public byte DiasTrabalho { get; set; } = 63;

    /// <summary>Período de ausência temporária (férias, licença). Sem horário disponível só nesse intervalo — antes e depois volta ao normal sozinho, sem precisar mexer em "Ativa/Inativa".</summary>
    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
