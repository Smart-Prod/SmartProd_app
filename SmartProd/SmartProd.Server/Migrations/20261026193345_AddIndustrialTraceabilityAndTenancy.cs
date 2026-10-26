using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartProd.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustrialTraceabilityAndTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_Code",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_NotasFiscais_Number",
                table: "NotasFiscais");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Vendas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Produtos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "OrdensProducao",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "NotasFiscais",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "NotaFiscalItems",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Movimentacoes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MateriaisItems",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Materiais",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Estoque",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ActorUserId = table.Column<int>(type: "integer", nullable: true),
                    ActorName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BeforeJson = table.Column<string>(type: "text", nullable: true),
                    AfterJson = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "integer", nullable: true),
                    Number = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SupplierLotNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ManufacturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lots_OrdensProducao_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Lots_Produtos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lots_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCenters_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotGenealogy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    InputLotId = table.Column<int>(type: "integer", nullable: false),
                    OutputLotId = table.Column<int>(type: "integer", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "integer", nullable: false),
                    QuantityConsumed = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotGenealogy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotGenealogy_Lots_InputLotId",
                        column: x => x.InputLotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotGenealogy_Lots_OutputLotId",
                        column: x => x.OutputLotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotGenealogy_OrdensProducao_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotGenealogy_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    WorkCenterId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Machines_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WorkCenterId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IdealCycleTimeSeconds = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOperations_Produtos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionOperations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOperations_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<int>(type: "integer", nullable: true),
                    MachineId = table.Column<int>(type: "integer", nullable: true),
                    OperatorId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GoodQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ScrapQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DeviceSequence = table.Column<long>(type: "bigint", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionEvents_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionEvents_OrdensProducao_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionEvents_ProductionOperations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "ProductionOperations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionEvents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionEvents_Usuarios_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "Slug" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SmartProd Demo", "smartprod-demo" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId",
                table: "Vendas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_Email",
                table: "Usuarios",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_Code",
                table: "Produtos",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_TenantId",
                table: "OrdensProducao",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_TenantId_Number",
                table: "NotasFiscais",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotaFiscalItems_TenantId",
                table: "NotaFiscalItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_TenantId",
                table: "Movimentacoes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MateriaisItems_TenantId",
                table: "MateriaisItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Materiais_TenantId",
                table: "Materiais",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_TenantId",
                table: "Estoque",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_EntityType_EntityId",
                table: "AuditEvents",
                columns: new[] { "TenantId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_OccurredAt",
                table: "AuditEvents",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LotGenealogy_InputLotId",
                table: "LotGenealogy",
                column: "InputLotId");

            migrationBuilder.CreateIndex(
                name: "IX_LotGenealogy_OutputLotId",
                table: "LotGenealogy",
                column: "OutputLotId");

            migrationBuilder.CreateIndex(
                name: "IX_LotGenealogy_ProductionOrderId",
                table: "LotGenealogy",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LotGenealogy_TenantId_InputLotId_OutputLotId_ProductionOrde~",
                table: "LotGenealogy",
                columns: new[] { "TenantId", "InputLotId", "OutputLotId", "ProductionOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lots_ProductId",
                table: "Lots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_ProductionOrderId",
                table: "Lots",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_TenantId_ProductId_Number",
                table: "Lots",
                columns: new[] { "TenantId", "ProductId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_TenantId_Code",
                table: "Machines",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkCenterId",
                table: "Machines",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_MachineId",
                table: "ProductionEvents",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_OperationId",
                table: "ProductionEvents",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_OperatorId",
                table: "ProductionEvents",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_OrderId",
                table: "ProductionEvents",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_TenantId_DeviceId_DeviceSequence",
                table: "ProductionEvents",
                columns: new[] { "TenantId", "DeviceId", "DeviceSequence" },
                unique: true,
                filter: "\"DeviceId\" IS NOT NULL AND \"DeviceSequence\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_TenantId_EventId",
                table: "ProductionEvents",
                columns: new[] { "TenantId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionEvents_TenantId_OrderId_OccurredAt",
                table: "ProductionEvents",
                columns: new[] { "TenantId", "OrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperations_ProductId",
                table: "ProductionOperations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperations_TenantId_ProductId_Sequence",
                table: "ProductionOperations",
                columns: new[] { "TenantId", "ProductId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperations_WorkCenterId",
                table: "ProductionOperations",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_TenantId_Code",
                table: "WorkCenters",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Estoque_Tenants_TenantId",
                table: "Estoque",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materiais_Tenants_TenantId",
                table: "Materiais",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MateriaisItems_Tenants_TenantId",
                table: "MateriaisItems",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimentacoes_Tenants_TenantId",
                table: "Movimentacoes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotaFiscalItems_Tenants_TenantId",
                table: "NotaFiscalItems",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscais_Tenants_TenantId",
                table: "NotasFiscais",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensProducao_Tenants_TenantId",
                table: "OrdensProducao",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Tenants_TenantId",
                table: "Produtos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Tenants_TenantId",
                table: "Vendas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoque_Tenants_TenantId",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "FK_Materiais_Tenants_TenantId",
                table: "Materiais");

            migrationBuilder.DropForeignKey(
                name: "FK_MateriaisItems_Tenants_TenantId",
                table: "MateriaisItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimentacoes_Tenants_TenantId",
                table: "Movimentacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_NotaFiscalItems_Tenants_TenantId",
                table: "NotaFiscalItems");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscais_Tenants_TenantId",
                table: "NotasFiscais");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensProducao_Tenants_TenantId",
                table: "OrdensProducao");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Tenants_TenantId",
                table: "Produtos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Tenants_TenantId",
                table: "Vendas");

            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "LotGenealogy");

            migrationBuilder.DropTable(
                name: "ProductionEvents");

            migrationBuilder.DropTable(
                name: "Lots");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "ProductionOperations");

            migrationBuilder.DropTable(
                name: "WorkCenters");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_TenantId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_TenantId_Code",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_OrdensProducao_TenantId",
                table: "OrdensProducao");

            migrationBuilder.DropIndex(
                name: "IX_NotasFiscais_TenantId_Number",
                table: "NotasFiscais");

            migrationBuilder.DropIndex(
                name: "IX_NotaFiscalItems_TenantId",
                table: "NotaFiscalItems");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacoes_TenantId",
                table: "Movimentacoes");

            migrationBuilder.DropIndex(
                name: "IX_MateriaisItems_TenantId",
                table: "MateriaisItems");

            migrationBuilder.DropIndex(
                name: "IX_Materiais_TenantId",
                table: "Materiais");

            migrationBuilder.DropIndex(
                name: "IX_Estoque_TenantId",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrdensProducao");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "NotasFiscais");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "NotaFiscalItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MateriaisItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Materiais");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Estoque");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Code",
                table: "Produtos",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_Number",
                table: "NotasFiscais",
                column: "Number",
                unique: true);
        }
    }
}
