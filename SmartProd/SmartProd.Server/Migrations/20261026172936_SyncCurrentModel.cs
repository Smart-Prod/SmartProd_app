using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartProd.API.Server.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoque_Produtos_ProdutoId",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscais_Usuarios_UsuarioId",
                table: "NotasFiscais");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensProducao_Produtos_ProdutoId",
                table: "OrdensProducao");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Produtos_ProdutoId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_ProdutoId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_OrdensProducao_ProdutoId",
                table: "OrdensProducao");

            migrationBuilder.DropIndex(
                name: "IX_Estoque_ProdutoId",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "OrdensProducao");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "MateriaisItems");

            migrationBuilder.DropColumn(
                name: "MateriasId",
                table: "MateriaisItems");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Estoque");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "OrdensProducao",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "NotasFiscais",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ProductId",
                table: "Vendas",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_ProductId",
                table: "OrdensProducao",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProductId",
                table: "Estoque",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estoque_Produtos_ProductId",
                table: "Estoque",
                column: "ProductId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscais_Usuarios_UsuarioId",
                table: "NotasFiscais",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensProducao_Produtos_ProductId",
                table: "OrdensProducao",
                column: "ProductId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Produtos_ProductId",
                table: "Vendas",
                column: "ProductId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoque_Produtos_ProductId",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscais_Usuarios_UsuarioId",
                table: "NotasFiscais");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensProducao_Produtos_ProductId",
                table: "OrdensProducao");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Produtos_ProductId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_ProductId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_OrdensProducao_ProductId",
                table: "OrdensProducao");

            migrationBuilder.DropIndex(
                name: "IX_Estoque_ProductId",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "OrdensProducao");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Vendas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "OrdensProducao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "NotasFiscais",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "MateriaisItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MateriasId",
                table: "MateriaisItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Estoque",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ProdutoId",
                table: "Vendas",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_ProdutoId",
                table: "OrdensProducao",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProdutoId",
                table: "Estoque",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estoque_Produtos_ProdutoId",
                table: "Estoque",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscais_Usuarios_UsuarioId",
                table: "NotasFiscais",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensProducao_Produtos_ProdutoId",
                table: "OrdensProducao",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Produtos_ProdutoId",
                table: "Vendas",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
