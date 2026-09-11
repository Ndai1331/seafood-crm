using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodAllocationLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "sales_allocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "sales_allocations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                INSERT INTO inventory_movements
                    ("InventoryBalanceId", "MovementType", "QuantityKg", "ReferenceType", "ReferenceId", "OccurredAt", "Reason")
                SELECT "Id", CASE WHEN "OnHandKg" >= 0 THEN 3 ELSE 9 END, "OnHandKg", 'LegacyBalance', "LotId", CURRENT_TIMESTAMP,
                       'Backfill từ tồn kho cũ khi bật inventory ledger'
                FROM inventory_balances
                WHERE "OnHandKg" <> 0
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "sales_allocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "sales_allocations");

            migrationBuilder.Sql("""
                DELETE FROM inventory_movements
                WHERE "ReferenceType" = 'LegacyBalance'
                """);
        }
    }
}
