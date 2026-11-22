using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itzyolotl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOpeningPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpeningPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<int>(type: "int", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    ClosesAt = table.Column<TimeOnly>(type: "time(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningPeriods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpeningPeriods_Day_OpensAt",
                table: "OpeningPeriods",
                columns: new[] { "Day", "OpensAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpeningPeriods");
        }
    }
}
