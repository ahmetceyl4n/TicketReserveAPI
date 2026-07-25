using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketReserve.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProcessedMessagePerConsumer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages");

            migrationBuilder.AddColumn<string>(
                name: "Consumer",
                table: "ProcessedMessages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages",
                columns: new[] { "MessageId", "Consumer" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages");

            migrationBuilder.DropColumn(
                name: "Consumer",
                table: "ProcessedMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages",
                column: "MessageId");
        }
    }
}
