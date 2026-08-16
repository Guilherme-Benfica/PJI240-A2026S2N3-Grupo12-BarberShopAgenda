using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/horarios")]
[Produces("application/json")]
public class HorariosController : ControllerBase
{
    private readonly IHorarioDisponivelService _horarioDisponivelService;

    public HorariosController(IHorarioDisponivelService horarioDisponivelService)
    {
        _horarioDisponivelService = horarioDisponivelService;
    }

    /// <summary>Lista os horários livres de um barbeiro em uma data, considerando a duração do serviço.</summary>
    [AllowAnonymous]
    [HttpGet("disponiveis")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<string>>> Disponiveis(
        [FromQuery] int barbeiroId, [FromQuery] DateOnly data, [FromQuery] int servicoId)
    {
        if (barbeiroId <= 0 || servicoId <= 0)
            return BadRequest(new { mensagem = "barbeiroId e servicoId são obrigatórios." });

        var horarios = await _horarioDisponivelService.ObterHorariosDisponiveisAsync(barbeiroId, data, servicoId);
        return Ok(horarios.Select(h => h.ToString("HH:mm")));
    }
}
