using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class ExpenseAndHouseCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Mevcut FKs ve PKs’i kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Houses_HouseId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_HouseMembers_Houses_HouseId",
                table: "HouseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Houses_HouseId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HouseMembers",
                table: "HouseMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Houses",
                table: "Houses");

            // 2) Kolon adlarını yeniden adlandır (IDENTITY korumak için RenameColumn)
            migrationBuilder.RenameColumn(
                name: "HouseId",
                table: "Houses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "HouseName",
                table: "Houses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Tur",
                table: "Expenses",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Tarih",
                table: "Expenses",
                newName: "CreatedDate");

            // 3) Artık kullanılmayacak CreatedBy sütununu kaldır
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Houses");

            // 4) Yeni sütunları ekle
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Houses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedDate",
                table: "HouseMembers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            // 5) PK’ları yeniden tanımla
            migrationBuilder.AddPrimaryKey(
                name: "PK_Houses",
                table: "Houses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HouseMembers",
                table: "HouseMembers",
                columns: new[] { "HouseId", "UserId" });

            // 6) FK’ları yeniden oluştur
            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Houses_HouseId",
                table: "Expenses",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HouseMembers_Houses_HouseId",
                table: "HouseMembers",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Houses_HouseId",
                table: "Invitations",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Oluşturulan FK’ları ve PK’ları kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Houses_HouseId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_HouseMembers_Houses_HouseId",
                table: "HouseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Houses_HouseId",
                table: "Expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HouseMembers",
                table: "HouseMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Houses",
                table: "Houses");

            // 2) Eklenen sütunları kaldır
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "JoinedDate",
                table: "HouseMembers");

            // 3) Eski CreatedBy sütununu geri ekle
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Houses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 4) Kolon adlarını geriye çevir
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Houses",
                newName: "HouseName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Houses",
                newName: "HouseId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Expenses",
                newName: "Tur");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Expenses",
                newName: "Tarih");

            // 5) PK’ları eski haline getir
            migrationBuilder.AddPrimaryKey(
                name: "PK_Houses",
                table: "Houses",
                column: "HouseId");

            // HouseMembers için önceden tek kolon (“Id”) vardı; eğer komutunuz
            // onu kaldırdıysa, aşağıdaki satırı da ekleyin:
            // migrationBuilder.AddColumn<int>(name: "Id", table: "HouseMembers", type: "int", nullable: false)
            //      .Annotation("SqlServer:Identity", "1, 1");
            migrationBuilder.AddPrimaryKey(
                name: "PK_HouseMembers",
                table: "HouseMembers",
                column: "Id");

            // 6) FK’ları eski haline getir
            migrationBuilder.CreateIndex(
                name: "IX_HouseMembers_HouseId",
                table: "HouseMembers",
                column: "HouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_HouseId",
                table: "Expenses",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Houses_HouseId",
                table: "Expenses",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HouseMembers_Houses_HouseId",
                table: "HouseMembers",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Houses_HouseId",
                table: "Invitations",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
