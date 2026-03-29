using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationFieldsOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcceptedByUserId",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Invitations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Invitations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedByUserId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Invitations");
        }
    }
}
