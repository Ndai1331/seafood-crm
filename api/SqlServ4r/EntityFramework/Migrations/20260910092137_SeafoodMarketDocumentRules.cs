using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodMarketDocumentRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "market_certificate_rules",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "market_certificate_rules");
        }
    }
}
