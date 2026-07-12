using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BatbyEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRateOverrideColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Students",
                type: "TEXT",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RateOverride",
                table: "Sessions",
                type: "TEXT",
                precision: 8,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RateOverride",
                table: "Sessions");
        }
    }
}
