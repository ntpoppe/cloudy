using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloudy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WhateverHappened : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StorageKey",
                table: "files",
                newName: "ObjectKey");

            migrationBuilder.RenameIndex(
                name: "IX_files_Bucket_StorageKey",
                table: "files",
                newName: "IX_files_Bucket_ObjectKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                table: "files",
                newName: "StorageKey");

            migrationBuilder.RenameIndex(
                name: "IX_files_Bucket_ObjectKey",
                table: "files",
                newName: "IX_files_Bucket_StorageKey");
        }
    }
}
