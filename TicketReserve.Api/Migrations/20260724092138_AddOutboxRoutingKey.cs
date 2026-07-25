using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketReserve.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxRoutingKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoutingKey",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutingKey",
                table: "OutboxMessages");
        }
    }
}
