using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop_Upgrade.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Discounts_Code",
                table: "Discounts");

            migrationBuilder.AddColumn<int>(
                name: "ProductColorId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Discounts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Discounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_Code",
                table: "Discounts",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Discounts_Code",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "ProductColorId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Discounts");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Discounts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_Code",
                table: "Discounts",
                column: "Code",
                unique: true);
        }
    }
}
