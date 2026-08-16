using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BarberShopAgenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "DiasTrabalho",
                table: "Barbeiros",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HorarioFimManha",
                table: "Barbeiros",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HorarioFimTarde",
                table: "Barbeiros",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HorarioInicioManha",
                table: "Barbeiros",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HorarioInicioTarde",
                table: "Barbeiros",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Barbeiros",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoConfirmacao",
                table: "Agendamentos",
                type: "varchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenhaHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Papel = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DiasTrabalho", "HorarioFimManha", "HorarioFimTarde", "HorarioInicioManha", "HorarioInicioTarde", "UsuarioId" },
                values: new object[] { (byte)63, new TimeOnly(12, 0, 0), new TimeOnly(19, 0, 0), new TimeOnly(9, 0, 0), new TimeOnly(13, 0, 0), 2 });

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DiasTrabalho", "HorarioFimManha", "HorarioFimTarde", "HorarioInicioManha", "HorarioInicioTarde", "UsuarioId" },
                values: new object[] { (byte)63, new TimeOnly(12, 0, 0), new TimeOnly(19, 0, 0), new TimeOnly(9, 0, 0), new TimeOnly(13, 0, 0), 3 });

            migrationBuilder.UpdateData(
                table: "Barbeiros",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DiasTrabalho", "HorarioFimManha", "HorarioFimTarde", "HorarioInicioManha", "HorarioInicioTarde", "UsuarioId" },
                values: new object[] { (byte)63, new TimeOnly(12, 0, 0), new TimeOnly(19, 0, 0), new TimeOnly(9, 0, 0), new TimeOnly(13, 0, 0), 4 });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Ativo", "DataCadastro", "Email", "Nome", "Papel", "SenhaHash" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@barbershop.com", "Administrador", 0, "AQAAAAIAAYagAAAAEALS4Lb5vWiYPmmQFgUKKs5kmYjZALMI7i4meu9fPlAxq15d8thqwG9Ns75FRbzA4g==" },
                    { 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "carlos.silva@barbershop.com", "Carlos Silva", 1, "AQAAAAIAAYagAAAAEB/8Wz4FFhsxpPNegqz3iqcE1lO4G46znRK6GQ7gGZY0J2WdKaeuHZlGUgu4GhLMTQ==" },
                    { 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "joao.pereira@barbershop.com", "João Pereira", 1, "AQAAAAIAAYagAAAAEBy7KwJ9F4W7DZzBXQle0B0NrCquufmoJIbVSNJ30VQBaMVTzhYjtGkHZ05KR1r19w==" },
                    { 4, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "marcos.souza@barbershop.com", "Marcos Souza", 1, "AQAAAAIAAYagAAAAEFeTriNcRDe10YZZpLPfi+s+UfCm1EwRMEbHlX355or+Y+IESPd6txvMMI7ajmtkqQ==" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barbeiros_UsuarioId",
                table: "Barbeiros",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Barbeiros_Usuarios_UsuarioId",
                table: "Barbeiros",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbeiros_Usuarios_UsuarioId",
                table: "Barbeiros");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Barbeiros_UsuarioId",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "DiasTrabalho",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "HorarioFimManha",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "HorarioFimTarde",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "HorarioInicioManha",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "HorarioInicioTarde",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Barbeiros");

            migrationBuilder.DropColumn(
                name: "CodigoConfirmacao",
                table: "Agendamentos");
        }
    }
}
