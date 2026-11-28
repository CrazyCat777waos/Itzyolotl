using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itzyolotl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StoreSettings_Capacity",
                table: "StoreSettings");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "StoreSettings",
                newName: "CrepesCapacity");

            migrationBuilder.AddColumn<int>(
                name: "BackCapacity",
                table: "StoreSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CoffeeAndPaninisCapacity",
                table: "StoreSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Station",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Station",
                table: "OrderLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "StoreSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BackCapacity", "CoffeeAndPaninisCapacity", "CrepesCapacity" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StoreSettings_BackCapacity",
                table: "StoreSettings",
                sql: "[BackCapacity] BETWEEN 1 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StoreSettings_CoffeeAndPaninisCapacity",
                table: "StoreSettings",
                sql: "[CoffeeAndPaninisCapacity] BETWEEN 1 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StoreSettings_CrepesCapacity",
                table: "StoreSettings",
                sql: "[CrepesCapacity] BETWEEN 1 AND 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StoreSettings_BackCapacity",
                table: "StoreSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StoreSettings_CoffeeAndPaninisCapacity",
                table: "StoreSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StoreSettings_CrepesCapacity",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "BackCapacity",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "CoffeeAndPaninisCapacity",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "Station",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Station",
                table: "OrderLines");

            migrationBuilder.RenameColumn(
                name: "CrepesCapacity",
                table: "StoreSettings",
                newName: "Capacity");

            migrationBuilder.UpdateData(
                table: "StoreSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Capacity",
                value: 2);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StoreSettings_Capacity",
                table: "StoreSettings",
                sql: "[Capacity] BETWEEN 1 AND 10");
        }
    }
}
