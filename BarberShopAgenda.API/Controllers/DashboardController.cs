using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Retorna o resumo do dia: total de agendamentos e receita prevista.</summary>
    [HttpGet("hoje")]
    [ProducesResponseType(typeof(ResumoDoDia), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoDoDia>> Hoje()
    {
        var resumo = await _dashboardService.GetResumoHojeAsync();
        return Ok(resumo);
    }
}
