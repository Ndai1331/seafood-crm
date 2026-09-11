using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodMarketRulesBackfillActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous migration added a non-null column to existing rows.
            // Existing seeded rules are valid and must remain visible after upgrade.
            migrationBuilder.Sql("UPDATE market_certificate_rules SET \"IsActive\" = TRUE WHERE \"IsActive\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
