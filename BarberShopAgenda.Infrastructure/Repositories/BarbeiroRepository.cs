using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Repositories;

public class BarbeiroRepository : IBarbeiroRepository
{
    private readonly BarberShopContext _context;

    public BarbeiroRepository(BarberShopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Barbeiro>> GetAllAsync()
        => await _context.Barbeiros.Include(b => b.Usuario).AsNoTracking().OrderBy(b => b.Nome).ToListAsync();

    public async Task<IEnumerable<Barbeiro>> GetAllVisiveisAsync()
        => await _context.Barbeiros.Include(b => b.Usuario).AsNoTracking()
            .Where(b => b.Usuario == null || b.Usuario.Ativo)
            .OrderBy(b => b.Nome)
            .ToListAsync();

    public async Task<Barbeiro?> GetByIdAsync(int id)
        => await _context.Barbeiros.Include(b => b.Usuario).AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Barbeiro> AddAsync(Barbeiro barbeiro)
    {
        _context.Barbeiros.Add(barbeiro);
        await _context.SaveChangesAsync();
        return barbeiro;
    }

    public async Task<bool> UpdateAsync(Barbeiro barbeiro)
    {
        var existente = await _context.Barbeiros.FindAsync(barbeiro.Id);
        if (existente is null) return false;

        // A tela de edição (barbeiros.html) sempre pré-preenche e reenvia os horários/dias atuais,
        // então é seguro sobrescrever tudo aqui — não corre o risco de zerar por omissão do DTO.
        existente.Nome = barbeiro.Nome;
        existente.Especialidade = barbeiro.Especialidade;
        existente.Ativo = barbeiro.Ativo;
        existente.HorarioInicioManha = barbeiro.HorarioInicioManha;
        existente.HorarioFimManha = barbeiro.HorarioFimManha;
        existente.HorarioInicioTarde = barbeiro.HorarioInicioTarde;
        existente.HorarioFimTarde = barbeiro.HorarioFimTarde;
        existente.DiasTrabalho = barbeiro.DiasTrabalho;
        existente.SabadoHorarioFim = barbeiro.SabadoHorarioFim;
        existente.FeriasInicio = barbeiro.FeriasInicio;
        existente.FeriasFim = barbeiro.FeriasFim;

        await _context.SaveChangesAsync();
        return true;
    }
}
