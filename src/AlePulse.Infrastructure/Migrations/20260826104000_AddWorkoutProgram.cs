using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlePulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutProgramId",
                table: "Workouts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "WorkoutPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPrograms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_WorkoutProgramId",
                table: "Workouts",
                column: "WorkoutProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPrograms_UserId",
                table: "WorkoutPrograms",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_WorkoutPrograms_WorkoutProgramId",
                table: "Workouts",
                column: "WorkoutProgramId",
                principalTable: "WorkoutPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_WorkoutPrograms_WorkoutProgramId",
                table: "Workouts");

            migrationBuilder.DropTable(
                name: "WorkoutPrograms");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_WorkoutProgramId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "WorkoutProgramId",
                table: "Workouts");
        }
    }
}
