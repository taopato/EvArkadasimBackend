using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorUserIdToHouse_Correct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
    name: "CreatorUserId",
    table: "Houses",
    type: "int",
    nullable: false,
    defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Houses_CreatorUserId",
                table: "Houses",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Houses_Users_CreatorUserId",
                table: "Houses",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
    name: "FK_Houses_Users_CreatorUserId",
    table: "Houses");

            migrationBuilder.DropIndex(
                name: "IX_Houses_CreatorUserId",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Houses");
        }
    }
}
