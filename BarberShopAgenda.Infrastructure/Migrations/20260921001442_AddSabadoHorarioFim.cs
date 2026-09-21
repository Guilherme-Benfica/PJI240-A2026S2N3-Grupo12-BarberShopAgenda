using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopAgenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSabadoHorarioFim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "SabadoHorarioFim",
                table: "Barbeiros",
                type: "time(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 1,
                column: "SabadoHorarioFim",
                value: new TimeOnly(16, 0, 0));

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 2,
                column: "SabadoHorarioFim",
                value: new TimeOnly(16, 0, 0));

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 3,
                column: "SabadoHorarioFim",
                value: new TimeOnly(16, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SabadoHorarioFim",
                table: "Barbeiros");
        }
    }
}
