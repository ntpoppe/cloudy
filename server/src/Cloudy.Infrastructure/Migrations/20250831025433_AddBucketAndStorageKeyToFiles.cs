using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloudy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBucketAndStorageKeyToFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "files");

            migrationBuilder.AddColumn<string>(
                name: "Bucket",
                table: "files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_files",
                table: "files",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_files_Bucket_StorageKey",
                table: "files",
                columns: new[] { "Bucket", "StorageKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_files",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_Bucket_StorageKey",
                table: "files");

            migrationBuilder.DropColumn(
                name: "Bucket",
                table: "files");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "files");

            migrationBuilder.RenameTable(
                name: "files",
                newName: "Files");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");
        }
    }
}
