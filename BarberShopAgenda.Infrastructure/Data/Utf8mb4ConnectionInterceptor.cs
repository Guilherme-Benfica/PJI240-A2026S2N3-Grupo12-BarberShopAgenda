using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BarberShopAgenda.Infrastructure.Data;

/// <summary>
/// Força a sessão MySQL a usar utf8mb4 logo após abrir a conexão.
/// Necessário porque o servidor (Aiven) usa latin1 como charset padrão de
/// conexão mesmo com as colunas em utf8mb4 — sem isso, todo texto acentuado
/// gravado pela aplicação (diferente do schema.sql, que já roda com
/// "SET NAMES utf8mb4" explícito) fica com os bytes corrompidos.
/// </summary>
public class Utf8mb4ConnectionInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var comando = connection.CreateCommand();
        comando.CommandText = "SET NAMES utf8mb4;";
        comando.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        using var comando = connection.CreateCommand();
        comando.CommandText = "SET NAMES utf8mb4;";
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}
