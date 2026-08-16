using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/barbeiros")]
[Produces("application/json")]
public class BarbeirosController : ControllerBase
{
    private readonly IBarbeiroRepository _barbeiroRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public BarbeirosController(IBarbeiroRepository barbeiroRepository, IUsuarioRepository usuarioRepository)
    {
        _barbeiroRepository = barbeiroRepository;
        _usuarioRepository = usuarioRepository;
    }

    private static BarbeiroResponseDTO ParaResponseDTO(Barbeiro b) => new()
    {
        Id = b.Id,
        Nome = b.Nome,
        Especialidade = b.Especialidade,
        Ativo = b.Ativo,
        HorarioInicioManha = b.HorarioInicioManha,
        HorarioFimManha = b.HorarioFimManha,
        HorarioInicioTarde = b.HorarioInicioTarde,
        HorarioFimTarde = b.HorarioFimTarde,
        DiasTrabalho = b.DiasTrabalho,
        FeriasInicio = b.FeriasInicio,
        FeriasFim = b.FeriasFim,
        Email = b.Usuario?.Email,
        ContaAtiva = b.Usuario?.Ativo
    };

    /// <summary>Lista os barbeiros visíveis publicamente (agenda + catálogo do cliente) — some quem tiver a conta inativa.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BarbeiroResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BarbeiroResponseDTO>>> GetAll()
    {
        var barbeiros = await _barbeiroRepository.GetAllVisiveisAsync();
        return Ok(barbeiros.Select(ParaResponseDTO));
    }

    /// <summary>Lista todos os barbeiros, inclusive com a conta inativa — uso administrativo.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("todos")]
    [ProducesResponseType(typeof(IEnumerable<BarbeiroResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BarbeiroResponseDTO>>> GetTodos()
    {
        var barbeiros = await _barbeiroRepository.GetAllAsync();
        return Ok(barbeiros.Select(ParaResponseDTO));
    }

    /// <summary>Busca um barbeiro pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BarbeiroResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbeiroResponseDTO>> GetById(int id)
    {
        var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
        if (barbeiro is null) return NotFound(new { mensagem = "Barbeiro não encontrado." });
        return Ok(ParaResponseDTO(barbeiro));
    }

    /// <summary>Cria um novo barbeiro, já com uma conta de login (e-mail/senha definidos pelo admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(BarbeiroResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BarbeiroResponseDTO>> Create(BarbeiroCreateDTO dto)
    {
        var existente = await _usuarioRepository.GetByEmailAsync(dto.Email);
        if (existente is not null)
            return BadRequest(new { mensagem = "Já existe uma conta com esse e-mail." });

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Papel = PapelUsuario.Barbeiro,
            Ativo = true,
            EmailConfirmado = true,
            DataCadastro = DateTime.UtcNow
        };
        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);
        var usuarioCriado = await _usuarioRepository.AddAsync(usuario);

        var barbeiro = new Barbeiro
        {
            Nome = dto.Nome,
            Especialidade = dto.Especialidade,
            Ativo = dto.Ativo,
            HorarioInicioManha = dto.HorarioInicioManha,
            HorarioFimManha = dto.HorarioFimManha,
            HorarioInicioTarde = dto.HorarioInicioTarde,
            HorarioFimTarde = dto.HorarioFimTarde,
            DiasTrabalho = dto.DiasTrabalho,
            FeriasInicio = dto.FeriasInicio,
            FeriasFim = dto.FeriasFim,
            UsuarioId = usuarioCriado.Id
        };

        var criado = await _barbeiroRepository.AddAsync(barbeiro);
        criado.Usuario = usuarioCriado;
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, ParaResponseDTO(criado));
    }

    /// <summary>Atualiza os dados de agenda de um barbeiro (nome, especialidade, situação da agenda, período de férias). Não mexe em horários/dias de trabalho nem na conta de login.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, BarbeiroUpdateDTO dto)
    {
        var barbeiro = new Barbeiro
        {
            Id = id,
            Nome = dto.Nome,
            Especialidade = dto.Especialidade,
            Ativo = dto.Ativo,
            FeriasInicio = dto.FeriasInicio,
            FeriasFim = dto.FeriasFim
        };

        var atualizado = await _barbeiroRepository.UpdateAsync(barbeiro);
        if (!atualizado) return NotFound(new { mensagem = "Barbeiro não encontrado." });
        return NoContent();
    }

    /// <summary>Ativa a conta de login do barbeiro (ele volta a poder entrar e a aparecer pra clientes/outros usuários).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/conta/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> AtivarConta(int id) => AlterarSituacaoDaContaAsync(id, ativo: true);

    /// <summary>Inativa a conta de login do barbeiro — ele deixa de conseguir entrar e some do catálogo público.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/conta/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> InativarConta(int id) => AlterarSituacaoDaContaAsync(id, ativo: false);

    private async Task<IActionResult> AlterarSituacaoDaContaAsync(int barbeiroId, bool ativo)
    {
        var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);
        if (barbeiro is null) return NotFound(new { mensagem = "Barbeiro não encontrado." });
        if (barbeiro.Usuario is null) return BadRequest(new { mensagem = "Este barbeiro não tem conta de login vinculada." });

        barbeiro.Usuario.Ativo = ativo;
        await _usuarioRepository.UpdateAsync(barbeiro.Usuario);
        return NoContent();
    }
}
