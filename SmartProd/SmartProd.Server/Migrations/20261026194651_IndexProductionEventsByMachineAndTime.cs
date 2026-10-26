using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartProd.Server.Migrations
{
    /// <inheritdoc />
    public partial class IndexProductionEventsByMachineAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_TenantId_MachineId_OccurredAt",
                table: "ProductionEvents",
                columns: new[] { "TenantId", "MachineId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionEvents_TenantId_MachineId_OccurredAt",
                table: "ProductionEvents");
        }
    }
}
