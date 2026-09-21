using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BarberShopAgenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServicosTabelaValores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nome",
                value: "Corte");

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Preco",
                value: 40.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Preco",
                value: 70.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 4,
                column: "Preco",
                value: 10.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descricao", "Nome", "Preco" },
                values: new object[] { "Pigmentação para barba ou cabelo — valor a partir de R$ 25,00, pode variar conforme o serviço", "Pigmentação", 25.00m });

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Descricao", "DuracaoMinutos", "Nome", "Preco" },
                values: new object[,]
                {
                    { 6, "Combo corte de cabelo e sobrancelha", 40, "Corte + Sobrancelha", 45.00m },
                    { 7, "Acabamento do corte (pezinho)", 15, "Pezinho", 20.00m },
                    { 8, "Combo barba e acabamento do corte", 30, "Barba + Pezinho", 50.00m },
                    { 9, "Mechas/luzes no cabelo", 120, "Luzes", 150.00m },
                    { 10, "Descoloração completa do cabelo", 150, "Platinado", 180.00m },
                    { 11, "Hidratação capilar", 30, "Hidratação", 20.00m },
                    { 12, "Remoção de pelos nasais com cera", 10, "Cera Nasal", 20.00m },
                    { 13, "Relaxamento capilar", 30, "Relaxamento", 25.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nome",
                value: "Corte de Cabelo");

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Preco",
                value: 30.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Preco",
                value: 60.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 4,
                column: "Preco",
                value: 15.00m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descricao", "Nome", "Preco" },
                values: new object[] { "Pigmentação para uniformizar a barba", "Pigmentação de Barba", 45.00m });
        }
    }
}
