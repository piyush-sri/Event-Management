using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventQR.Migrations
{
    /// <inheritdoc />
    public partial class GuestCheckinScannerIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_TicketScanners_ScannerLoginId",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_ScannerLoginId",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "ScannerLoginId",
                table: "CheckIns");

            migrationBuilder.AlterColumn<string>(
                name: "UserLoginId",
                table: "CheckIns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_UserLoginId",
                table: "CheckIns",
                column: "UserLoginId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_AppUser_UserLoginId",
                table: "CheckIns",
                column: "UserLoginId",
                principalTable: "AppUser",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_AppUser_UserLoginId",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_UserLoginId",
                table: "CheckIns");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserLoginId",
                table: "CheckIns",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScannerLoginId",
                table: "CheckIns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ScannerLoginId",
                table: "CheckIns",
                column: "ScannerLoginId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_TicketScanners_ScannerLoginId",
                table: "CheckIns",
                column: "ScannerLoginId",
                principalTable: "TicketScanners",
                principalColumn: "UniqueId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
