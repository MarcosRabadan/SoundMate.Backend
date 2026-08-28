using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoundMate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AcademySoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Academies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Academies_DeletedAtUtc",
                table: "Academies",
                column: "DeletedAtUtc",
                filter: "\"DeletedAtUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Academies_DeletedAtUtc",
                table: "Academies");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Academies");
        }
    }
}
