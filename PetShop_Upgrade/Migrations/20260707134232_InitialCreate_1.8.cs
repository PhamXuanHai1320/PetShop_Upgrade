using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop_Upgrade.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingWard",
                table: "Orders",
                newName: "ShippingWardName");

            migrationBuilder.RenameColumn(
                name: "ShippingCity",
                table: "Orders",
                newName: "ShippingWardCode");

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledByAdminId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCityCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCityName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledByAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCityCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCityName",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShippingWardName",
                table: "Orders",
                newName: "ShippingWard");

            migrationBuilder.RenameColumn(
                name: "ShippingWardCode",
                table: "Orders",
                newName: "ShippingCity");
        }
    }
}
