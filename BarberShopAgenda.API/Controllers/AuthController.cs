using BarberShopAgenda.API.DTOs;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopAgenda.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IClienteContaService _clienteContaService;

    public AuthController(IAuthService authService, ITokenService tokenService, IClienteContaService clienteContaService)
    {
        _authService = authService;
        _tokenService = tokenService;
        _clienteContaService = clienteContaService;
    }

    /// <summary>Autentica um usuário (Admin, Barbeiro ou Cliente) e retorna um token JWT.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDTO>> Login(AuthLoginDTO dto)
    {
        var usuario = await _authService.AutenticarAsync(dto.Email, dto.Senha);
        if (usuario is null) return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        return Ok(new AuthResponseDTO
        {
            Token = token,
            ExpiraEm = expiraEm,
            Nome = usuario.Nome,
            Papel = usuario.Papel.ToString(),
            BarbeiroId = usuario.Barbeiro?.Id,
            ClienteId = usuario.Cliente?.Id
        });
    }

    /// <summary>Cria uma conta de cliente. Se o telefone já tiver agendamentos como convidado, vincula o histórico existente.</summary>
    [AllowAnonymous]
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(RegistrarClienteDTO dto)
    {
        await _clienteContaService.RegistrarAsync(dto.Nome, dto.Telefone, dto.Email, dto.Senha);
        return StatusCode(StatusCodes.Status201Created, new { mensagem = "Conta criada. Verifique seu e-mail para confirmar o cadastro." });
    }

    /// <summary>Confirma o e-mail de uma conta de cliente a partir do token enviado por e-mail.</summary>
    [AllowAnonymous]
    [HttpPost("confirmar-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmarEmail(ConfirmarEmailDTO dto)
    {
        var confirmado = await _clienteContaService.ConfirmarEmailAsync(dto.Token);
        if (!confirmado) return BadRequest(new { mensagem = "Token inválido ou expirado." });
        return Ok(new { mensagem = "E-mail confirmado. Você já pode entrar." });
    }

    /// <summary>Solicita a redefinição de senha. Sempre responde com sucesso, mesmo que o e-mail não exista.</summary>
    [AllowAnonymous]
    [HttpPost("esqueci-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaDTO dto)
    {
        await _clienteContaService.SolicitarRedefinicaoSenhaAsync(dto.Email);
        return Ok(new { mensagem = "Se esse e-mail estiver cadastrado, enviamos um link de redefinição." });
    }

    /// <summary>Redefine a senha a partir do token enviado por e-mail.</summary>
    [AllowAnonymous]
    [HttpPost("redefinir-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaDTO dto)
    {
        var redefinida = await _clienteContaService.RedefinirSenhaAsync(dto.Token, dto.NovaSenha);
        if (!redefinida) return BadRequest(new { mensagem = "Token inválido ou expirado." });
        return Ok(new { mensagem = "Senha redefinida com sucesso." });
    }

    /// <summary>Troca a senha do usuário autenticado (Admin, Barbeiro ou Cliente), informando a senha atual.</summary>
    [Authorize]
    [HttpPut("senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaDTO dto)
    {
        var usuarioIdClaim = User.FindFirst("usuarioId")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var alterada = await _authService.AlterarSenhaAsync(usuarioId, dto.SenhaAtual, dto.NovaSenha);
        if (!alterada) return BadRequest(new { mensagem = "Senha atual incorreta." });
        return Ok(new { mensagem = "Senha alterada com sucesso." });
    }
}
