using System.Globalization;
using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/agendamentos")]
[Produces("application/json")]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly IClienteRepository _clienteRepository;

    public AgendamentosController(IAgendamentoService agendamentoService, IClienteRepository clienteRepository)
    {
        _agendamentoService = agendamentoService;
        _clienteRepository = clienteRepository;
    }

    private static AgendamentoResponseDTO ParaResponseDTO(Agendamento a) => new()
    {
        Id = a.Id,
        ClienteId = a.ClienteId,
        ClienteNome = a.Cliente?.Nome ?? string.Empty,
        BarbeiroId = a.BarbeiroId,
        BarbeiroNome = a.Barbeiro?.Nome ?? string.Empty,
        ServicoId = a.ServicoId,
        ServicoNome = a.Servico?.Nome ?? string.Empty,
        ServicoPreco = a.Servico?.Preco ?? 0,
        ServicoDuracaoMinutos = a.Servico?.DuracaoMinutos ?? 0,
        DataHora = a.DataHora,
        Status = a.Status,
        Observacao = a.Observacao,
        CodigoConfirmacao = a.CodigoConfirmacao
    };

    /// <summary>Lista todos os agendamentos.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AgendamentoResponseDTO>>> GetAll()
    {
        var agendamentos = await _agendamentoService.GetAllAsync();
        return Ok(agendamentos.Select(ParaResponseDTO));
    }

    /// <summary>Busca um agendamento pelo id.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AgendamentoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendamentoResponseDTO>> GetById(int id)
    {
        var agendamento = await _agendamentoService.GetByIdAsync(id);
        if (agendamento is null) return NotFound(new { mensagem = "Agendamento não encontrado." });
        return Ok(ParaResponseDTO(agendamento));
    }

    /// <summary>Lista a agenda de um barbeiro específico. Um usuário com papel Barbeiro só pode consultar a própria agenda.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpGet("barbeiro/{barbeiroId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AgendamentoResponseDTO>>> GetByBarbeiro(int barbeiroId)
    {
        if (User.IsInRole("Barbeiro") && !User.IsInRole("Admin"))
        {
            var barbeiroIdClaim = User.FindFirst("barbeiroId")?.Value;
            if (barbeiroIdClaim != barbeiroId.ToString())
                return Forbid();
        }

        var agendamentos = await _agendamentoService.GetByBarbeiroAsync(barbeiroId);
        return Ok(agendamentos.Select(ParaResponseDTO));
    }

    /// <summary>
    /// Lista os agendamentos (passados e futuros) de um cliente, buscado pelo telefone.
    /// Exige também o código de confirmação de algum agendamento já feito por esse cliente — o telefone
    /// sozinho não é segredo, então usamos o código (gerado aleatoriamente, só exibido para quem agendou)
    /// como comprovação de que quem está consultando é realmente o dono do telefone.
    /// Endpoint público — usado pela tela "Meus agendamentos".
    /// </summary>
    [AllowAnonymous]
    [HttpGet("cliente")]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AgendamentoResponseDTO>>> GetByCliente([FromQuery] string telefone, [FromQuery] string codigo)
    {
        const string mensagemNaoEncontrado = "Nenhum agendamento encontrado para esse telefone e código de confirmação.";

        if (string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(codigo))
            return NotFound(new { mensagem = mensagemNaoEncontrado });

        var cliente = await _clienteRepository.GetByTelefoneAsync(telefone);
        if (cliente is null) return NotFound(new { mensagem = mensagemNaoEncontrado });

        var agendamentos = (await _agendamentoService.GetByClienteAsync(cliente.Id)).ToList();

        var possuiCodigoValido = agendamentos.Any(a =>
            string.Equals(a.CodigoConfirmacao, codigo.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!possuiCodigoValido) return NotFound(new { mensagem = mensagemNaoEncontrado });

        return Ok(agendamentos.Select(ParaResponseDTO));
    }

    /// <summary>Lista os agendamentos do cliente autenticado (conta com login), lendo o id direto do token.</summary>
    [Authorize(Roles = "Cliente")]
    [HttpGet("me")]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AgendamentoResponseDTO>>> GetMeusAgendamentos()
    {
        var clienteIdClaim = User.FindFirst("clienteId")?.Value;
        if (!int.TryParse(clienteIdClaim, out var clienteId))
            return Ok(Array.Empty<AgendamentoResponseDTO>());

        var agendamentos = await _agendamentoService.GetByClienteAsync(clienteId);
        return Ok(agendamentos.Select(ParaResponseDTO));
    }

    /// <summary>Lista a agenda de uma data específica (formato yyyy-MM-dd).</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpGet("data/{data}")]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AgendamentoResponseDTO>>> GetByData(string data)
    {
        if (!DateTime.TryParseExact(data, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataParseada))
            return BadRequest(new { mensagem = "Data inválida. Utilize o formato yyyy-MM-dd." });

        var agendamentos = await _agendamentoService.GetByDataAsync(dataParseada);
        return Ok(agendamentos.Select(ParaResponseDTO));
    }

    /// <summary>Cria um novo agendamento, validando conflito de horário. Endpoint público — usado no fluxo de agendamento do cliente.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(AgendamentoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgendamentoResponseDTO>> Create(AgendamentoCreateDTO dto)
    {
        var criado = await _agendamentoService.CriarAsync(dto.ClienteId, dto.BarbeiroId, dto.ServicoId, dto.DataHora, dto.Observacao);
        var agendamentoCompleto = await _agendamentoService.GetByIdAsync(criado.Id);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, ParaResponseDTO(agendamentoCompleto!));
    }

    /// <summary>Confirma um agendamento.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpPut("{id:int}/confirmar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(int id)
    {
        var confirmado = await _agendamentoService.ConfirmarAsync(id);
        if (!confirmado) return NotFound(new { mensagem = "Agendamento não encontrado." });
        return NoContent();
    }

    /// <summary>Cancela um agendamento.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpPut("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelado = await _agendamentoService.CancelarAsync(id);
        if (!cancelado) return NotFound(new { mensagem = "Agendamento não encontrado." });
        return NoContent();
    }

    /// <summary>Marca um agendamento como concluído.</summary>
    [Authorize(Roles = "Admin,Barbeiro")]
    [HttpPut("{id:int}/concluir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Concluir(int id)
    {
        var concluido = await _agendamentoService.ConcluirAsync(id);
        if (!concluido) return NotFound(new { mensagem = "Agendamento não encontrado." });
        return NoContent();
    }
}
