using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly BarberShopContext _context;

    public ServicoRepository(BarberShopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Servico>> GetAllAsync()
        => await _context.Servicos.AsNoTracking().OrderBy(s => s.Nome).ToListAsync();

    public async Task<Servico?> GetByIdAsync(int id)
        => await _context.Servicos.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Servico> AddAsync(Servico servico)
    {
        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task<bool> UpdateAsync(Servico servico)
    {
        var existente = await _context.Servicos.FindAsync(servico.Id);
        if (existente is null) return false;

        existente.Nome = servico.Nome;
        existente.Descricao = servico.Descricao;
        existente.Preco = servico.Preco;
        existente.DuracaoMinutos = servico.DuracaoMinutos;

        await _context.SaveChangesAsync();
        return true;
    }
}
