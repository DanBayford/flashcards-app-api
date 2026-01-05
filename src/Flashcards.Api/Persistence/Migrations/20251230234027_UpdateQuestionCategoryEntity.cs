using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flashcards.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionCategoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionCategories_categories_CategoryId",
                table: "QuestionCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionCategories_questions_QuestionId",
                table: "QuestionCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionCategories",
                table: "QuestionCategories");

            migrationBuilder.RenameTable(
                name: "QuestionCategories",
                newName: "question_categories");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionCategories_CategoryId",
                table: "question_categories",
                newName: "IX_question_categories_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_question_categories",
                table: "question_categories",
                columns: new[] { "QuestionId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_question_categories_categories_CategoryId",
                table: "question_categories",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_question_categories_questions_QuestionId",
                table: "question_categories",
                column: "QuestionId",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_categories_categories_CategoryId",
                table: "question_categories");

            migrationBuilder.DropForeignKey(
                name: "FK_question_categories_questions_QuestionId",
                table: "question_categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_question_categories",
                table: "question_categories");

            migrationBuilder.RenameTable(
                name: "question_categories",
                newName: "QuestionCategories");

            migrationBuilder.RenameIndex(
                name: "IX_question_categories_CategoryId",
                table: "QuestionCategories",
                newName: "IX_QuestionCategories_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionCategories",
                table: "QuestionCategories",
                columns: new[] { "QuestionId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionCategories_categories_CategoryId",
                table: "QuestionCategories",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionCategories_questions_QuestionId",
                table: "QuestionCategories",
                column: "QuestionId",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
