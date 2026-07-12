using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BatbyEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentBookingDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultDay",
                table: "Students",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DefaultStartTime",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultSubject",
                table: "Students",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultTutorId",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultDay",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DefaultStartTime",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DefaultSubject",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DefaultTutorId",
                table: "Students");
        }
    }
}
