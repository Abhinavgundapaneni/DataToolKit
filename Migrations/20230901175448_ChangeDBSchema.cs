using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataToolKit.Migrations
{
    public partial class ChangeDBSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customerName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customerName",
                table: "AspNetUsers");
        }
    }
}
