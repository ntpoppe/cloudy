using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloudy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPendingDeletion",
                table: "folders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPendingDeletion",
                table: "files",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPendingDeletion",
                table: "folders");

            migrationBuilder.DropColumn(
                name: "IsPendingDeletion",
                table: "files");
        }
    }
}
