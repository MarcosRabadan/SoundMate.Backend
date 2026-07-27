using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoundMate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Academies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Plan = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Academies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherDisciplines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDisciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherGenres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherGenres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherReviews", x => x.Id);
                    table.CheckConstraint("CK_TeacherReviews_Stars", "[Stars] >= 1 AND [Stars] <= 5");
                });

            migrationBuilder.CreateTable(
                name: "UserDisciplines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDisciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEducations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEducations", x => x.Id);
                    table.CheckConstraint("CK_UserEducations_Years", "[StartYear] IS NULL OR [EndYear] IS NULL OR [EndYear] >= [StartYear]");
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EmailVerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Disciplines",
                columns: new[] { "Id", "Category", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), 1, true, "Piano" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), 1, true, "Keyboard" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), 1, true, "Organ" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), 1, true, "Accordion" },
                    { new Guid("20000000-0000-0000-0000-000000000001"), 2, true, "Classical guitar" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), 2, true, "Acoustic guitar" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), 2, true, "Electric guitar" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), 2, true, "Flamenco guitar" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), 2, true, "Electric bass" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), 2, true, "Ukulele" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), 2, true, "Banjo" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), 2, true, "Mandolin" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), 2, true, "Harp" },
                    { new Guid("20000000-0000-0000-0000-00000000000a"), 2, true, "Bandurria" },
                    { new Guid("20000000-0000-0000-0000-00000000000b"), 2, true, "Lute" },
                    { new Guid("30000000-0000-0000-0000-000000000001"), 3, true, "Violin" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), 3, true, "Viola" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), 3, true, "Cello" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), 3, true, "Double bass" },
                    { new Guid("40000000-0000-0000-0000-000000000001"), 4, true, "Flute" },
                    { new Guid("40000000-0000-0000-0000-000000000002"), 4, true, "Recorder" },
                    { new Guid("40000000-0000-0000-0000-000000000003"), 4, true, "Clarinet" },
                    { new Guid("40000000-0000-0000-0000-000000000004"), 4, true, "Saxophone" },
                    { new Guid("40000000-0000-0000-0000-000000000005"), 4, true, "Oboe" },
                    { new Guid("40000000-0000-0000-0000-000000000006"), 4, true, "Bassoon" },
                    { new Guid("50000000-0000-0000-0000-000000000001"), 5, true, "Trumpet" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), 5, true, "Trombone" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), 5, true, "French horn" },
                    { new Guid("50000000-0000-0000-0000-000000000004"), 5, true, "Tuba" },
                    { new Guid("50000000-0000-0000-0000-000000000005"), 5, true, "Euphonium" },
                    { new Guid("50000000-0000-0000-0000-000000000006"), 5, true, "Cornet" },
                    { new Guid("50000000-0000-0000-0000-000000000007"), 5, true, "Flugelhorn" },
                    { new Guid("60000000-0000-0000-0000-000000000001"), 6, true, "Drum kit" },
                    { new Guid("60000000-0000-0000-0000-000000000002"), 6, true, "Snare drum" },
                    { new Guid("60000000-0000-0000-0000-000000000003"), 6, true, "Cajón" },
                    { new Guid("60000000-0000-0000-0000-000000000004"), 6, true, "Congas" },
                    { new Guid("60000000-0000-0000-0000-000000000005"), 6, true, "Bongos" },
                    { new Guid("60000000-0000-0000-0000-000000000006"), 6, true, "Djembe" },
                    { new Guid("60000000-0000-0000-0000-000000000007"), 6, true, "Timpani" },
                    { new Guid("60000000-0000-0000-0000-000000000008"), 6, true, "Mallet percussion" },
                    { new Guid("70000000-0000-0000-0000-000000000001"), 7, true, "Singing" },
                    { new Guid("70000000-0000-0000-0000-000000000002"), 7, true, "Classical voice" },
                    { new Guid("80000000-0000-0000-0000-000000000001"), 8, true, "Music theory" },
                    { new Guid("80000000-0000-0000-0000-000000000002"), 8, true, "Harmony" },
                    { new Guid("80000000-0000-0000-0000-000000000003"), 8, true, "Composition" },
                    { new Guid("80000000-0000-0000-0000-000000000004"), 8, true, "Music analysis" },
                    { new Guid("80000000-0000-0000-0000-000000000005"), 8, true, "Music history" },
                    { new Guid("80000000-0000-0000-0000-000000000006"), 8, true, "Improvisation" }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), true, "Classical" },
                    { new Guid("90000000-0000-0000-0000-000000000002"), true, "Opera" },
                    { new Guid("90000000-0000-0000-0000-000000000003"), true, "Baroque" },
                    { new Guid("90000000-0000-0000-0000-000000000004"), true, "Contemporary classical" },
                    { new Guid("90000000-0000-0000-0000-000000000005"), true, "Jazz" },
                    { new Guid("90000000-0000-0000-0000-000000000006"), true, "Blues" },
                    { new Guid("90000000-0000-0000-0000-000000000007"), true, "Swing" },
                    { new Guid("90000000-0000-0000-0000-000000000008"), true, "Bossa nova" },
                    { new Guid("90000000-0000-0000-0000-000000000009"), true, "Rock" },
                    { new Guid("90000000-0000-0000-0000-00000000000a"), true, "Hard rock" },
                    { new Guid("90000000-0000-0000-0000-00000000000b"), true, "Metal" },
                    { new Guid("90000000-0000-0000-0000-00000000000c"), true, "Punk" },
                    { new Guid("90000000-0000-0000-0000-00000000000d"), true, "Indie" },
                    { new Guid("90000000-0000-0000-0000-00000000000e"), true, "Pop" },
                    { new Guid("90000000-0000-0000-0000-00000000000f"), true, "Funk" },
                    { new Guid("90000000-0000-0000-0000-000000000010"), true, "Soul" },
                    { new Guid("90000000-0000-0000-0000-000000000011"), true, "R&B" },
                    { new Guid("90000000-0000-0000-0000-000000000012"), true, "Hip hop" },
                    { new Guid("90000000-0000-0000-0000-000000000013"), true, "Rap" },
                    { new Guid("90000000-0000-0000-0000-000000000014"), true, "Reggae" },
                    { new Guid("90000000-0000-0000-0000-000000000015"), true, "Ska" },
                    { new Guid("90000000-0000-0000-0000-000000000016"), true, "Reggaeton" },
                    { new Guid("90000000-0000-0000-0000-000000000017"), true, "Electronic" },
                    { new Guid("90000000-0000-0000-0000-000000000018"), true, "House" },
                    { new Guid("90000000-0000-0000-0000-000000000019"), true, "Techno" },
                    { new Guid("90000000-0000-0000-0000-00000000001a"), true, "Folk" },
                    { new Guid("90000000-0000-0000-0000-00000000001b"), true, "Country" },
                    { new Guid("90000000-0000-0000-0000-00000000001c"), true, "Flamenco" },
                    { new Guid("90000000-0000-0000-0000-00000000001d"), true, "Latin" },
                    { new Guid("90000000-0000-0000-0000-00000000001e"), true, "Salsa" },
                    { new Guid("90000000-0000-0000-0000-00000000001f"), true, "Tango" },
                    { new Guid("90000000-0000-0000-0000-000000000020"), true, "Gospel" },
                    { new Guid("90000000-0000-0000-0000-000000000021"), true, "Film music" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Academies_OwnerId",
                table: "Academies",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Academies_Slug",
                table: "Academies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Name",
                table: "Disciplines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_AcademyId",
                table: "Memberships",
                column: "AcademyId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId_AcademyId_Role",
                table: "Memberships",
                columns: new[] { "UserId", "AcademyId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDisciplines_DisciplineId",
                table: "TeacherDisciplines",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDisciplines_UserId_DisciplineId",
                table: "TeacherDisciplines",
                columns: new[] { "UserId", "DisciplineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherGenres_GenreId",
                table: "TeacherGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherGenres_UserId_GenreId",
                table: "TeacherGenres",
                columns: new[] { "UserId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReviews_ReviewerUserId_TeacherUserId_AcademyId",
                table: "TeacherReviews",
                columns: new[] { "ReviewerUserId", "TeacherUserId", "AcademyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReviews_TeacherUserId_AcademyId",
                table: "TeacherReviews",
                columns: new[] { "TeacherUserId", "AcademyId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDisciplines_DisciplineId",
                table: "UserDisciplines",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDisciplines_UserId_DisciplineId",
                table: "UserDisciplines",
                columns: new[] { "UserId", "DisciplineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEducations_UserId",
                table: "UserEducations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Academies");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "TeacherDisciplines");

            migrationBuilder.DropTable(
                name: "TeacherGenres");

            migrationBuilder.DropTable(
                name: "TeacherReviews");

            migrationBuilder.DropTable(
                name: "UserDisciplines");

            migrationBuilder.DropTable(
                name: "UserEducations");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
