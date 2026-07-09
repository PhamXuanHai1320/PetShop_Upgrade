using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop_Upgrade.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_AddressId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AddressId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AddressId1",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressId1",
                table: "Orders",
                column: "AddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_AddressId1",
                table: "Orders",
                column: "AddressId1",
                principalTable: "Addresses",
                principalColumn: "Id");
        }
    }
}
