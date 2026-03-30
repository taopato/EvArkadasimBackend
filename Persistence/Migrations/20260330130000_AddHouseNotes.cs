using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Contexts;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260330130000_AddHouseNotes")]
    public partial class AddHouseNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HouseNoteSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseNoteSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HouseNoteSections_Houses_HouseId",
                        column: x => x.HouseId,
                        principalTable: "Houses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseNoteSections_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HouseNoteItems_HouseNoteSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "HouseNoteSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseNoteItems_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HouseNoteItems_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteItems_CompletedByUserId",
                table: "HouseNoteItems",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteItems_CreatedByUserId",
                table: "HouseNoteItems",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteItems_SectionId_IsCompleted_DeletedAt",
                table: "HouseNoteItems",
                columns: new[] { "SectionId", "IsCompleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteSections_CreatedByUserId",
                table: "HouseNoteSections",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteSections_HouseId_Title",
                table: "HouseNoteSections",
                columns: new[] { "HouseId", "Title" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseNoteItems");

            migrationBuilder.DropTable(
                name: "HouseNoteSections");
        }
    }
}
