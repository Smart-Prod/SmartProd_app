using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartProd.API.Server.Migrations;

public partial class AddUsuarioRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RefreshTokenHash",
            table: "Usuarios",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenExpiresAt",
            table: "Usuarios",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Usuarios_RefreshTokenHash",
            table: "Usuarios",
            column: "RefreshTokenHash",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Usuarios_RefreshTokenHash", table: "Usuarios");
        migrationBuilder.DropColumn(name: "RefreshTokenHash", table: "Usuarios");
        migrationBuilder.DropColumn(name: "RefreshTokenExpiresAt", table: "Usuarios");
    }
}
