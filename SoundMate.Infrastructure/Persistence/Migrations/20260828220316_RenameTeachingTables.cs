using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoundMate.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Renames the two "person ↔ discipline" tables and the genre one so the table says which
    /// relationship it holds: <c>StudiedDisciplines</c> is what somebody studies (with a level),
    /// <c>TaughtDisciplines</c> and <c>TaughtGenres</c> are what they teach.
    /// <para>
    /// Hand-written. EF scaffolds a rename as drop-then-create, which would throw the rows away;
    /// these are renames, so the data and the indexes survive. Postgres does not carry constraint
    /// names along with the table either, so each primary key is renamed explicitly —
    /// <c>ALTER INDEX</c> renames the constraint behind it.
    /// </para>
    /// </summary>
    public partial class RenameTeachingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserDisciplines",
                newName: "StudiedDisciplines");

            migrationBuilder.RenameIndex(
                name: "PK_UserDisciplines",
                table: "StudiedDisciplines",
                newName: "PK_StudiedDisciplines");

            migrationBuilder.RenameIndex(
                name: "IX_UserDisciplines_DisciplineId",
                table: "StudiedDisciplines",
                newName: "IX_StudiedDisciplines_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "IX_UserDisciplines_UserId_DisciplineId",
                table: "StudiedDisciplines",
                newName: "IX_StudiedDisciplines_UserId_DisciplineId");

            migrationBuilder.RenameTable(
                name: "TeacherDisciplines",
                newName: "TaughtDisciplines");

            migrationBuilder.RenameIndex(
                name: "PK_TeacherDisciplines",
                table: "TaughtDisciplines",
                newName: "PK_TaughtDisciplines");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDisciplines_DisciplineId",
                table: "TaughtDisciplines",
                newName: "IX_TaughtDisciplines_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDisciplines_UserId_DisciplineId",
                table: "TaughtDisciplines",
                newName: "IX_TaughtDisciplines_UserId_DisciplineId");

            migrationBuilder.RenameTable(
                name: "TeacherGenres",
                newName: "TaughtGenres");

            migrationBuilder.RenameIndex(
                name: "PK_TeacherGenres",
                table: "TaughtGenres",
                newName: "PK_TaughtGenres");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherGenres_GenreId",
                table: "TaughtGenres",
                newName: "IX_TaughtGenres_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherGenres_UserId_GenreId",
                table: "TaughtGenres",
                newName: "IX_TaughtGenres_UserId_GenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_TaughtGenres_UserId_GenreId",
                table: "TaughtGenres",
                newName: "IX_TeacherGenres_UserId_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_TaughtGenres_GenreId",
                table: "TaughtGenres",
                newName: "IX_TeacherGenres_GenreId");

            migrationBuilder.RenameIndex(
                name: "PK_TaughtGenres",
                table: "TaughtGenres",
                newName: "PK_TeacherGenres");

            migrationBuilder.RenameTable(
                name: "TaughtGenres",
                newName: "TeacherGenres");

            migrationBuilder.RenameIndex(
                name: "IX_TaughtDisciplines_UserId_DisciplineId",
                table: "TaughtDisciplines",
                newName: "IX_TeacherDisciplines_UserId_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "IX_TaughtDisciplines_DisciplineId",
                table: "TaughtDisciplines",
                newName: "IX_TeacherDisciplines_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "PK_TaughtDisciplines",
                table: "TaughtDisciplines",
                newName: "PK_TeacherDisciplines");

            migrationBuilder.RenameTable(
                name: "TaughtDisciplines",
                newName: "TeacherDisciplines");

            migrationBuilder.RenameIndex(
                name: "IX_StudiedDisciplines_UserId_DisciplineId",
                table: "StudiedDisciplines",
                newName: "IX_UserDisciplines_UserId_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "IX_StudiedDisciplines_DisciplineId",
                table: "StudiedDisciplines",
                newName: "IX_UserDisciplines_DisciplineId");

            migrationBuilder.RenameIndex(
                name: "PK_StudiedDisciplines",
                table: "StudiedDisciplines",
                newName: "PK_UserDisciplines");

            migrationBuilder.RenameTable(
                name: "StudiedDisciplines",
                newName: "UserDisciplines");
        }
    }
}
