using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodAuditBackfillLegacyStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE apphistories SET \"StatusCode\" = 200 WHERE \"StatusCode\" = 0");
            migrationBuilder.Sql("UPDATE apphistories SET \"Succeeded\" = TRUE WHERE \"StatusCode\" = 200 AND \"Succeeded\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
