using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasco.ProjectService.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessDate",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "AccessStatus",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedStatus",
                table: "ProjectMembers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedUpdateDate",
                table: "ProjectMembers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedStatus",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "ApprovedUpdateDate",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessDate",
                table: "ProjectMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AccessStatus",
                table: "ProjectMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
