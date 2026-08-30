using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Domain.Interfaces;
using BarberShopAgenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly BarberShopContext _context;

    public UsuarioRepository(BarberShopContext context)
    {
        _context = context;
    }

    private IQueryable<Usuario> QueryComIncludes()
        => _context.Usuarios.Include(u => u.Barbeiro).Include(u => u.Cliente);

    public async Task<Usuario?> GetByEmailAsync(string email)
        => await QueryComIncludes().AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public async Task<Usuario?> GetByIdAsync(int id)
        => await QueryComIncludes().AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> GetByTokenVerificacaoAsync(string token)
        => await QueryComIncludes().AsNoTracking().FirstOrDefaultAsync(u => u.TokenVerificacaoEmail == token);

    public async Task<Usuario?> GetByTokenResetSenhaAsync(string token)
        => await QueryComIncludes().AsNoTracking().FirstOrDefaultAsync(u => u.TokenResetSenha == token);

    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        var existente = await _context.Usuarios.FindAsync(usuario.Id);
        if (existente is null) return false;

        existente.Nome = usuario.Nome;
        existente.Email = usuario.Email;
        existente.SenhaHash = usuario.SenhaHash;
        existente.Ativo = usuario.Ativo;
        existente.EmailConfirmado = usuario.EmailConfirmado;
        existente.TokenVerificacaoEmail = usuario.TokenVerificacaoEmail;
        existente.TokenVerificacaoExpiraEm = usuario.TokenVerificacaoExpiraEm;
        existente.TokenResetSenha = usuario.TokenResetSenha;
        existente.TokenResetSenhaExpiraEm = usuario.TokenResetSenhaExpiraEm;

        await _context.SaveChangesAsync();
        return true;
    }
}
