using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itzyolotl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderLinePreparationMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreparationMinutes",
                table: "OrderLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparationMinutes",
                table: "OrderLines");
        }
    }
}
