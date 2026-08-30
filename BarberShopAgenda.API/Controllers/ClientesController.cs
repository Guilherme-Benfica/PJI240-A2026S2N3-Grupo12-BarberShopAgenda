using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    private static ClienteResponseDTO ParaResponseDTO(Cliente c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Telefone = c.Telefone,
        Email = c.Email,
        DataCadastro = c.DataCadastro
    };

    /// <summary>Lista todos os clientes.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteResponseDTO>>> GetAll()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        return Ok(clientes.Select(ParaResponseDTO));
    }

    /// <summary>Busca um cliente pelo id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponseDTO>> GetById(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente is null) return NotFound(new { mensagem = "Cliente não encontrado." });
        return Ok(ParaResponseDTO(cliente));
    }

    /// <summary>
    /// Cria um novo cliente, ou reaproveita um já existente com o mesmo telefone (atualizando nome/e-mail).
    /// Endpoint público — usado no autocadastro do fluxo de agendamento, evitando duplicar o mesmo cliente a cada visita.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ClienteResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponseDTO>> Create(ClienteCreateDTO dto)
    {
        var existente = await _clienteRepository.GetByTelefoneAsync(dto.Telefone);
        if (existente is not null)
        {
            existente.Nome = dto.Nome;
            existente.Email = dto.Email;
            await _clienteRepository.UpdateAsync(existente);
            return Ok(ParaResponseDTO(existente));
        }

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            Email = dto.Email,
            DataCadastro = DateTime.UtcNow
        };

        var criado = await _clienteRepository.AddAsync(cliente);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, ParaResponseDTO(criado));
    }

    /// <summary>Atualiza um cliente existente.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ClienteUpdateDTO dto)
    {
        var cliente = new Cliente
        {
            Id = id,
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            Email = dto.Email
        };

        var atualizado = await _clienteRepository.UpdateAsync(cliente);
        if (!atualizado) return NotFound(new { mensagem = "Cliente não encontrado." });
        return NoContent();
    }

    /// <summary>Remove um cliente.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _clienteRepository.DeleteAsync(id);
        if (!removido) return NotFound(new { mensagem = "Cliente não encontrado." });
        return NoContent();
    }
}
