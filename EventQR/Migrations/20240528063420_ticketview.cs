using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventQR.Migrations
{
    /// <inheritdoc />
    public partial class ticketview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketViewName",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketViewName",
                table: "Events");
        }
    }
}
