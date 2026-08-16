using BarberShopAgenda.Domain.Entities;
using BarberShopAgenda.Infrastructure.Data;
using BarberShopAgenda.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarberShopAgenda.Tests.Services;

public class ClienteRepositoryTests
{
    private static ClienteRepository CriarRepositorio(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<BarberShopContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new ClienteRepository(new BarberShopContext(options));
    }

    [Fact]
    public async Task GetByTelefoneAsync_ComTelefoneCadastrado_DeveRetornarCliente()
    {
        var repository = CriarRepositorio(nameof(GetByTelefoneAsync_ComTelefoneCadastrado_DeveRetornarCliente));
        await repository.AddAsync(new Cliente { Nome = "Cliente Teste", Telefone = "11999990000", DataCadastro = DateTime.UtcNow });

        var encontrado = await repository.GetByTelefoneAsync("11999990000");

        Assert.NotNull(encontrado);
        Assert.Equal("Cliente Teste", encontrado!.Nome);
    }

    [Fact]
    public async Task GetByTelefoneAsync_ComTelefoneNaoCadastrado_DeveRetornarNulo()
    {
        var repository = CriarRepositorio(nameof(GetByTelefoneAsync_ComTelefoneNaoCadastrado_DeveRetornarNulo));

        var encontrado = await repository.GetByTelefoneAsync("00000000000");

        Assert.Null(encontrado);
    }
}
