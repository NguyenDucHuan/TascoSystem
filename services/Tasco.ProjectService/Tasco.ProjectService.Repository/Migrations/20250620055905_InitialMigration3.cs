using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasco.ProjectService.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "ProjectMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveDate",
                table: "ProjectMembers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessDate",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "AccessStatus",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "RemoveDate",
                table: "ProjectMembers");
        }
    }
}
