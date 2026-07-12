using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BatbyEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Students",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            // Set all existing students to active
            migrationBuilder.Sql("UPDATE Students SET IsActive = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Students");
        }
    }
}
