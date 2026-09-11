using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodDocumentOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_attachments_inbound_purchases_InboundPurchaseId",
                table: "document_attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_document_attachments_production_lots_ProductionLotId",
                table: "document_attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_document_attachments_raw_material_lots_RawMaterialLotId",
                table: "document_attachments");

            migrationBuilder.DropIndex(
                name: "IX_document_attachments_InboundPurchaseId",
                table: "document_attachments");

            migrationBuilder.DropIndex(
                name: "IX_document_attachments_ProductionLotId",
                table: "document_attachments");

            migrationBuilder.DropIndex(
                name: "IX_document_attachments_RawMaterialLotId",
                table: "document_attachments");

            migrationBuilder.DropColumn(
                name: "InboundPurchaseId",
                table: "document_attachments");

            migrationBuilder.DropColumn(
                name: "ProductionLotId",
                table: "document_attachments");

            migrationBuilder.DropColumn(
                name: "RawMaterialLotId",
                table: "document_attachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InboundPurchaseId",
                table: "document_attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionLotId",
                table: "document_attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawMaterialLotId",
                table: "document_attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_InboundPurchaseId",
                table: "document_attachments",
                column: "InboundPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_ProductionLotId",
                table: "document_attachments",
                column: "ProductionLotId");

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_RawMaterialLotId",
                table: "document_attachments",
                column: "RawMaterialLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_document_attachments_inbound_purchases_InboundPurchaseId",
                table: "document_attachments",
                column: "InboundPurchaseId",
                principalTable: "inbound_purchases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_attachments_production_lots_ProductionLotId",
                table: "document_attachments",
                column: "ProductionLotId",
                principalTable: "production_lots",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_attachments_raw_material_lots_RawMaterialLotId",
                table: "document_attachments",
                column: "RawMaterialLotId",
                principalTable: "raw_material_lots",
                principalColumn: "Id");
        }
    }
}
