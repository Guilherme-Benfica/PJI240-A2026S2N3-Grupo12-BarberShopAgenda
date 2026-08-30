using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopAgenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeriasBarbeiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FeriasFim",
                table: "Barbeiros",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FeriasInicio",
                table: "Barbeiros",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FeriasFim", "FeriasInicio" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FeriasFim", "FeriasInicio" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FeriasFim", "FeriasInicio" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeriasFim",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "FeriasInicio",
                table: "Barbeiros");
        }
    }
}
