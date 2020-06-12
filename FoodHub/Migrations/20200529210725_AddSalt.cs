using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodHub.Migrations
{
    public partial class AddSalt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "Restaurant",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Restaurant");
        }
    }
}
