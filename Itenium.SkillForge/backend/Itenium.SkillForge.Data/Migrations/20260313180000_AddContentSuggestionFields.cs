using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itenium.SkillForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSuggestionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmitterName",
                table: "ContentSuggestions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "ContentSuggestions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ContentSuggestions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "ContentSuggestions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentSuggestions_TeamId_Status",
                table: "ContentSuggestions",
                columns: new[] { "TeamId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentSuggestions_TeamId_Status",
                table: "ContentSuggestions");

            migrationBuilder.DropColumn(
                name: "SubmitterName",
                table: "ContentSuggestions");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "ContentSuggestions");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ContentSuggestions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "ContentSuggestions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
