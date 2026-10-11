using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class BarbeiroRepositoryTests
{
    private static BarbeiroRepository CriarRepositorio(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new BarbeiroRepository(new BarberShopContext(options));
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarHorariosEDiasDeTrabalho()
    {
        var repository = CriarRepositorio(nameof(UpdateAsync_DeveAtualizarHorariosEDiasDeTrabalho));
        var criado = await repository.AddAsync(new Barbeiro
        {
            Nome = "Barbeiro Teste",
            Ativo = true,
            HorarioInicioManha = new TimeOnly(9, 0),
            HorarioFimManha = new TimeOnly(12, 0),
            DiasTrabalho = 63
        });

        var atualizado = await repository.UpdateAsync(new Barbeiro
        {
            Id = criado.Id,
            Nome = "Barbeiro Teste",
            Ativo = true,
            HorarioInicioManha = new TimeOnly(10, 0),
            HorarioFimManha = new TimeOnly(13, 0),
            HorarioInicioTarde = new TimeOnly(14, 0),
            HorarioFimTarde = new TimeOnly(18, 0),
            DiasTrabalho = 31 // Seg a Sex
        });

        var persistido = await repository.GetByIdAsync(criado.Id);

        Assert.True(atualizado);
        Assert.Equal(new TimeOnly(10, 0), persistido!.HorarioInicioManha);
        Assert.Equal(new TimeOnly(13, 0), persistido.HorarioFimManha);
        Assert.Equal(new TimeOnly(14, 0), persistido.HorarioInicioTarde);
        Assert.Equal(new TimeOnly(18, 0), persistido.HorarioFimTarde);
        Assert.Equal((byte)31, persistido.DiasTrabalho);
    }

    [Fact]
    public async Task UpdateAsync_ComIdInexistente_DeveRetornarFalso()
    {
        var repository = CriarRepositorio(nameof(UpdateAsync_ComIdInexistente_DeveRetornarFalso));

        var atualizado = await repository.UpdateAsync(new Barbeiro { Id = 999, Nome = "Não existe" });

        Assert.False(atualizado);
    }
}
