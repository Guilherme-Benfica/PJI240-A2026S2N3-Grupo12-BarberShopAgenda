using BarberShopAgenda.Domain;
using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BarberShopAgenda.Infrastructure.Services;

public class AgendamentoService : IAgendamentoService
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IBarbeiroRepository _barbeiroRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IEmailService _emailService;
    private readonly IClienteContaService _clienteContaService;
    private readonly ILogger<AgendamentoService> _logger;

    public AgendamentoService(
        IAgendamentoRepository agendamentoRepository,
        IClienteRepository clienteRepository,
        IBarbeiroRepository barbeiroRepository,
        IServicoRepository servicoRepository,
        IEmailService emailService,
        IClienteContaService clienteContaService,
        ILogger<AgendamentoService> logger)
    {
        _agendamentoRepository = agendamentoRepository;
        _clienteRepository = clienteRepository;
        _barbeiroRepository = barbeiroRepository;
        _servicoRepository = servicoRepository;
        _emailService = emailService;
        _clienteContaService = clienteContaService;
        _logger = logger;
    }

    public Task<IEnumerable<Agendamento>> GetAllAsync() => _agendamentoRepository.GetAllAsync();

    public Task<Agendamento?> GetByIdAsync(int id) => _agendamentoRepository.GetByIdAsync(id);

    public Task<IEnumerable<Agendamento>> GetByBarbeiroAsync(int barbeiroId) => _agendamentoRepository.GetByBarbeiroAsync(barbeiroId);

    public Task<IEnumerable<Agendamento>> GetByClienteAsync(int clienteId) => _agendamentoRepository.GetByClienteAsync(clienteId);

    public Task<IEnumerable<Agendamento>> GetByDataAsync(DateTime data) => _agendamentoRepository.GetByDataAsync(data);

    public async Task<Agendamento> CriarAsync(int clienteId, int barbeiroId, int servicoId, DateTime dataHora, string? observacao)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId)
            ?? throw new RegraNegocioException("Cliente não encontrado.");

        var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId)
            ?? throw new RegraNegocioException("Barbeiro não encontrado.");

        if (!barbeiro.Ativo)
            throw new RegraNegocioException("Barbeiro não está ativo.");

        var servico = await _servicoRepository.GetByIdAsync(servicoId)
            ?? throw new RegraNegocioException("Serviço não encontrado.");

        if (dataHora < HorarioBrasil.Agora.AddMinutes(-1))
            throw new RegraNegocioException("Não é possível agendar em uma data/hora no passado.");

        var conflitoBarbeiro = await _agendamentoRepository.ExisteConflitoAsync(barbeiroId, dataHora, servico.DuracaoMinutos);
        if (conflitoBarbeiro)
            throw new ConflitoHorarioException("Já existe um agendamento para este barbeiro nesse horário.");

        var conflitoCliente = await _agendamentoRepository.ExisteConflitoClienteAsync(clienteId, dataHora, servico.DuracaoMinutos);
        if (conflitoCliente)
            throw new ConflitoHorarioException("Você já tem um agendamento marcado nesse horário.");

        var agendamento = new Agendamento
        {
            ClienteId = clienteId,
            BarbeiroId = barbeiroId,
            ServicoId = servicoId,
            DataHora = dataHora,
            Status = StatusAgendamento.Pendente,
            Observacao = observacao,
            CodigoConfirmacao = GerarCodigoConfirmacao()
        };

        var criado = await _agendamentoRepository.AddAsync(agendamento);

        if (!string.IsNullOrWhiteSpace(cliente.Email))
        {
            try
            {
                await _emailService.EnviarConfirmacaoAgendamentoAsync(
                    cliente.Email, cliente.Nome, servico.Nome, barbeiro.Nome, dataHora, criado.CodigoConfirmacao);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar e-mail de confirmação para o agendamento {AgendamentoId}.", criado.Id);
            }
        }

        try
        {
            await _clienteContaService.GarantirContaVinculadaAsync(clienteId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao criar conta automática para o cliente {ClienteId}.", clienteId);
        }

        return criado;
    }

    private static string GerarCodigoConfirmacao()
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6).Select(_ => caracteres[Random.Shared.Next(caracteres.Length)]).ToArray());
    }

    public async Task<bool> ConfirmarAsync(int id)
    {
        var agendamento = await _agendamentoRepository.GetByIdAsync(id);
        if (agendamento is null) return false;

        if (agendamento.Status == StatusAgendamento.Cancelado)
            throw new RegraNegocioException("Não é possível confirmar um agendamento cancelado.");

        agendamento.Status = StatusAgendamento.Confirmado;
        return await _agendamentoRepository.UpdateAsync(agendamento);
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var agendamento = await _agendamentoRepository.GetByIdAsync(id);
        if (agendamento is null) return false;

        agendamento.Status = StatusAgendamento.Cancelado;
        return await _agendamentoRepository.UpdateAsync(agendamento);
    }

    public async Task<bool> ConcluirAsync(int id)
    {
        var agendamento = await _agendamentoRepository.GetByIdAsync(id);
        if (agendamento is null) return false;

        if (agendamento.Status == StatusAgendamento.Cancelado)
            throw new RegraNegocioException("Não é possível concluir um agendamento cancelado.");

        agendamento.Status = StatusAgendamento.Concluido;
        return await _agendamentoRepository.UpdateAsync(agendamento);
    }
}
