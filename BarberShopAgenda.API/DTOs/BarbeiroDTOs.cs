using System.ComponentModel.DataAnnotations;

namespace BarberShopAgenda.API.DTOs;

public class BarbeiroCreateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Especialidade { get; set; }

    public bool Ativo { get; set; } = true;

    public TimeOnly? HorarioInicioManha { get; set; }
    public TimeOnly? HorarioFimManha { get; set; }
    public TimeOnly? HorarioInicioTarde { get; set; }
    public TimeOnly? HorarioFimTarde { get; set; }
    public byte DiasTrabalho { get; set; } = 63;

    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }

    /// <summary>E-mail de login do barbeiro. Obrigatório só na criação — o cadastro já sai com conta pronta pra entrar.</summary>
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>
/// Atualiza os dados de agenda editáveis pelo admin. Não inclui horários/dias de trabalho —
/// esses só são definidos na criação, e não há tela hoje para editá-los depois (evita zerá-los sem querer).
/// </summary>
public class BarbeiroUpdateDTO
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Especialidade { get; set; }

    public bool Ativo { get; set; } = true;

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
    public DateOnly? FeriasInicio { get; set; }
    public DateOnly? FeriasFim { get; set; }
    public string? Email { get; set; }

    /// <summary>Situação da conta de login. Null quando o barbeiro não tem conta vinculada.</summary>
    public bool? ContaAtiva { get; set; }
}
