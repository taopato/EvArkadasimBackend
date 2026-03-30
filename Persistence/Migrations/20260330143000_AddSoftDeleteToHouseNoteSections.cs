using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Contexts;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260330143000_AddSoftDeleteToHouseNoteSections")]
    public partial class AddSoftDeleteToHouseNoteSections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HouseNoteSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "HouseNoteSections",
                type: "int",
                nullable: true);

            migrationBuilder.DropIndex(
                name: "IX_HouseNoteSections_HouseId_Title",
                table: "HouseNoteSections");

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteSections_HouseId_DeletedAt_Title",
                table: "HouseNoteSections",
                columns: new[] { "HouseId", "DeletedAt", "Title" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HouseNoteSections_HouseId_DeletedAt_Title",
                table: "HouseNoteSections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HouseNoteSections");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "HouseNoteSections");

            migrationBuilder.CreateIndex(
                name: "IX_HouseNoteSections_HouseId_Title",
                table: "HouseNoteSections",
                columns: new[] { "HouseId", "Title" });
        }
    }
}
