using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoundMate.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Gives <c>StudiedDiscipline</c> the <c>CreatedAtUtc</c> / <c>UpdatedAtUtc</c> pair the other
    /// aggregates already carry, ahead of the <c>SaveChanges</c> interceptor that will fill them.
    /// <para>
    /// The columns are not nullable, so existing rows need a value. EF scaffolds
    /// <c>0001-01-01</c> for that, which would read as "created at the dawn of the calendar" for
    /// every row already there; <c>now()</c> is not the truth either, but it is the closest
    /// available and it sorts sanely. The default is then <b>dropped</b>: it existed only to
    /// backfill, and leaving it would let a hand-written INSERT silently acquire a timestamp the
    /// aggregate never set.
    /// </para>
    /// </summary>
    public partial class StudiedDisciplineTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "StudiedDisciplines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "StudiedDisciplines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.Sql(
                """
                ALTER TABLE "StudiedDisciplines"
                    ALTER COLUMN "CreatedAtUtc" DROP DEFAULT,
                    ALTER COLUMN "UpdatedAtUtc" DROP DEFAULT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "StudiedDisciplines");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "StudiedDisciplines");
        }
    }
}
