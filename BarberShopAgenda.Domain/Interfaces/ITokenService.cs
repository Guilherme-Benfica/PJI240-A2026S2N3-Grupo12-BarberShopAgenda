using BarberShopAgenda.Domain.Entities;

namespace BarberShopAgenda.Domain.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
