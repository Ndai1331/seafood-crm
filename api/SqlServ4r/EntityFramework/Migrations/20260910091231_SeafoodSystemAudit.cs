using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodSystemAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "apphistories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "apphistories",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DurationMs",
                table: "apphistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "apphistories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "apphistories",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "apphistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Succeeded",
                table: "apphistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "apphistories",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "Succeeded",
                table: "apphistories");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "apphistories");
        }
    }
}
