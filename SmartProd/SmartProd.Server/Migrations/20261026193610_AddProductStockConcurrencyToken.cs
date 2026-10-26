using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartProd.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStockConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // xmin is a built-in PostgreSQL system column and must not be created by a migration.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL owns xmin; rolling back the model mapping requires no database operation.
        }
    }
}
