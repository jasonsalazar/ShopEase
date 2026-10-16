using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopEase.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCartKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CartKey",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CartKey",
                table: "AspNetUsers",
                column: "CartKey",
                unique: true,
                filter: "[CartKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CartKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CartKey",
                table: "AspNetUsers");
        }
    }
}
