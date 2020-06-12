using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodHub.Migrations
{
    public partial class AddStripeIDandPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeId",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payment",
                table: "Order",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Payment",
                table: "Order");
        }
    }
}
