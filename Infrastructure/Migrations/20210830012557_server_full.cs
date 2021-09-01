using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class server_full : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "greetingType",
                table: "Servers",
                newName: "GreetingType");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Servers",
                newName: "GreetingChannelId");

            migrationBuilder.AddColumn<ulong>(
                name: "CommandChannelId",
                table: "Servers",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandChannelId",
                table: "Servers");

            migrationBuilder.RenameColumn(
                name: "GreetingType",
                table: "Servers",
                newName: "greetingType");

            migrationBuilder.RenameColumn(
                name: "GreetingChannelId",
                table: "Servers",
                newName: "ChannelId");
        }
    }
}
