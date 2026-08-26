using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoundMate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAtUtc",
                table: "Users",
                column: "DeletedAtUtc",
                filter: "\"DeletedAtUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DeletedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Users");
        }
    }
}
