using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnBase.API.Migrations
{
    /// <inheritdoc />
    public partial class FixJunctionTableKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PracticeSetExercises",
                table: "PracticeSetExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseTags",
                table: "ExerciseTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PracticeSetExercises",
                table: "PracticeSetExercises",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseTags",
                table: "ExerciseTags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSetExercise_SetId_ExerciseId_Unique",
                table: "PracticeSetExercises",
                columns: new[] { "PracticeSetId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseTag_ExerciseId_TagId_Unique",
                table: "ExerciseTags",
                columns: new[] { "ExerciseId", "TagId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PracticeSetExercises",
                table: "PracticeSetExercises");

            migrationBuilder.DropIndex(
                name: "IX_PracticeSetExercise_SetId_ExerciseId_Unique",
                table: "PracticeSetExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseTags",
                table: "ExerciseTags");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseTag_ExerciseId_TagId_Unique",
                table: "ExerciseTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PracticeSetExercises",
                table: "PracticeSetExercises",
                columns: new[] { "PracticeSetId", "ExerciseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseTags",
                table: "ExerciseTags",
                columns: new[] { "ExerciseId", "TagId" });
        }
    }
}
