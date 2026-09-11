using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodInboundCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ColdStorageFeeUsd",
                table: "inbound_purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomsFeeUsd",
                table: "inbound_purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HandlingFeeUsd",
                table: "inbound_purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InfrastructureFeeUsd",
                table: "inbound_purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReleaseOrderFeeUsd",
                table: "inbound_purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColdStorageFeeUsd",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "CustomsFeeUsd",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "HandlingFeeUsd",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "InfrastructureFeeUsd",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "ReleaseOrderFeeUsd",
                table: "inbound_purchases");
        }
    }
}
