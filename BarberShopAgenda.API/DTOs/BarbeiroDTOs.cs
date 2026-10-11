using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.API.DTOs;

public class BarbeiroCreateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Especialidade { get; set; }

    public bool Ativo { get; set; } = true;

    public TimeOnly? HorarioInicioManha { get; set; }
    public TimeOnly? HorarioFimManha { get; set; }
    public TimeOnly? HorarioInicioTarde { get; set; }
    public TimeOnly? HorarioFimTarde { get; set; }
    public byte DiasTrabalho { get; set; } = 63;

    /// <summary>Fim do expediente aos sábados, quando diferente do resto da semana. Nulo = mesmo horário dos outros dias.</summary>
    public TimeOnly? SabadoHorarioFim { get; set; }

    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }

    /// <summary>E-mail de login do barbeiro. Obrigatório só na criação — o cadastro já sai com conta pronta pra entrar.</summary>
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>
/// Atualiza os dados de agenda editáveis pelo admin, incluindo horários/dias de trabalho.
/// O formulário (barbeiros.html) sempre envia os valores atuais do barbeiro, mesmo quando
/// o admin não mexeu neles — por isso é seguro incluir esses campos aqui sem risco de zerá-los.
/// </summary>
public class BarbeiroUpdateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Especialidade { get; set; }

    public bool Ativo { get; set; } = true;

    public TimeOnly? HorarioInicioManha { get; set; }
    public TimeOnly? HorarioFimManha { get; set; }
    public TimeOnly? HorarioInicioTarde { get; set; }
    public TimeOnly? HorarioFimTarde { get; set; }
    public byte DiasTrabalho { get; set; } = 63;

    /// <summary>Fim do expediente aos sábados, quando diferente do resto da semana. Nulo = mesmo horário dos outros dias.</summary>
    public TimeOnly? SabadoHorarioFim { get; set; }

    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }
}

public class BarbeiroResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Especialidade { get; set; }

    /// <summary>Situação da agenda: se falso, não aparece pra clientes agendarem — mas a conta continua podendo entrar.</summary>
    public bool Ativo { get; set; }

    public TimeOnly? HorarioInicioManha { get; set; }
    public TimeOnly? HorarioFimManha { get; set; }
    public TimeOnly? HorarioInicioTarde { get; set; }
    public TimeOnly? HorarioFimTarde { get; set; }
    public byte DiasTrabalho { get; set; }
    public TimeOnly? SabadoHorarioFim { get; set; }
    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }
    public string? Email { get; set; }

    /// <summary>Situação da conta de login. Null quando o barbeiro não tem conta vinculada.</summary>
    public bool? ContaAtiva { get; set; }
}
