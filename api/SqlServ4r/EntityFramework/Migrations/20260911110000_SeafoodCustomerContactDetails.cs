using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SqlServ4r.EntityFramework;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    [DbContext(typeof(DreamContext))]
    [Migration("20260911110000_SeafoodCustomerContactDetails")]
    public partial class SeafoodCustomerContactDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "customers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNo",
                table: "customers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "customers",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "customers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "customers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Address", table: "customers");
            migrationBuilder.DropColumn(name: "BusinessRegistrationNo", table: "customers");
            migrationBuilder.DropColumn(name: "CountryCode", table: "customers");
            migrationBuilder.DropColumn(name: "Email", table: "customers");
            migrationBuilder.DropColumn(name: "Phone", table: "customers");
            migrationBuilder.DropColumn(name: "TaxCode", table: "customers");
        }
    }
}
