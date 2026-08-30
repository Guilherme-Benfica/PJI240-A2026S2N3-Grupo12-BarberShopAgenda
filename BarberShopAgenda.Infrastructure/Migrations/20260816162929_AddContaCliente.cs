using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopAgenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TokenResetSenha",
                table: "Usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenResetSenhaExpiraEm",
                table: "Usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenVerificacaoEmail",
                table: "Usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenVerificacaoExpiraEm",
                table: "Usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EmailConfirmado", "TokenResetSenha", "TokenResetSenhaExpiraEm", "TokenVerificacaoEmail", "TokenVerificacaoExpiraEm" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EmailConfirmado", "TokenResetSenha", "TokenResetSenhaExpiraEm", "TokenVerificacaoEmail", "TokenVerificacaoExpiraEm" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EmailConfirmado", "TokenResetSenha", "TokenResetSenhaExpiraEm", "TokenVerificacaoEmail", "TokenVerificacaoExpiraEm" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EmailConfirmado", "TokenResetSenha", "TokenResetSenhaExpiraEm", "TokenVerificacaoEmail", "TokenVerificacaoExpiraEm" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EmailConfirmado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenResetSenha",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenResetSenhaExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenVerificacaoEmail",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenVerificacaoExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Clientes");
        }
    }
}
