using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/servicos")]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly IServicoRepository _servicoRepository;

    public ServicosController(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    private static ServicoResponseDTO ParaResponseDTO(Servico s) => new()
    {
        Id = s.Id,
        Nome = s.Nome,
        Descricao = s.Descricao,
        Preco = s.Preco,
        DuracaoMinutos = s.DuracaoMinutos
    };

    /// <summary>Lista todos os serviços.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServicoResponseDTO>>> GetAll()
    {
        var servicos = await _servicoRepository.GetAllAsync();
        return Ok(servicos.Select(ParaResponseDTO));
    }

    /// <summary>Busca um serviço pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServicoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicoResponseDTO>> GetById(int id)
    {
        var servico = await _servicoRepository.GetByIdAsync(id);
        if (servico is null) return NotFound(new { mensagem = "Serviço não encontrado." });
        return Ok(ParaResponseDTO(servico));
    }

    /// <summary>Cria um novo serviço.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServicoResponseDTO>> Create(ServicoCreateDTO dto)
    {
        var servico = new Servico
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            DuracaoMinutos = dto.DuracaoMinutos
        };

        var criado = await _servicoRepository.AddAsync(servico);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, ParaResponseDTO(criado));
    }

    /// <summary>Atualiza um serviço existente.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ServicoUpdateDTO dto)
    {
        var servico = new Servico
        {
            Id = id,
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            DuracaoMinutos = dto.DuracaoMinutos
        };

        var atualizado = await _servicoRepository.UpdateAsync(servico);
        if (!atualizado) return NotFound(new { mensagem = "Serviço não encontrado." });
        return NoContent();
    }
}
