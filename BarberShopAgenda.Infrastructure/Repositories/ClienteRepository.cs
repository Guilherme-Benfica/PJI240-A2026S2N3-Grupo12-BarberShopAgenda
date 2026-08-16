using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly BarberShopContext _context;

    public ClienteRepository(BarberShopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
        => await _context.Clientes.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();

    public async Task<Cliente?> GetByIdAsync(int id)
        => await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Cliente?> GetByTelefoneAsync(string telefone)
        => await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Telefone == telefone);

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> UpdateAsync(Cliente cliente)
    {
        var existente = await _context.Clientes.FindAsync(cliente.Id);
        if (existente is null) return false;

        existente.Nome = cliente.Nome;
        existente.Telefone = cliente.Telefone;
        existente.Email = cliente.Email;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VincularUsuarioAsync(int clienteId, int usuarioId)
    {
        var existente = await _context.Clientes.FindAsync(clienteId);
        if (existente is null) return false;

        existente.UsuarioId = usuarioId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existente = await _context.Clientes.FindAsync(id);
        if (existente is null) return false;

        _context.Clientes.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
