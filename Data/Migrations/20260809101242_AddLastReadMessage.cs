using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedCircle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReadMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastReadMessageId",
                table: "ConversationMembers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadMessageId",
                table: "ConversationMembers");
        }
    }
}
