using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasco.ProjectService.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "ProjectMembers");

            migrationBuilder.RenameColumn(
                name: "ApprovedStatus",
                table: "ProjectMembers",
                newName: "Status");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "ProjectMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RemovedBy",
                table: "ProjectMembers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "RemovedBy",
                table: "ProjectMembers");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ProjectMembers",
                newName: "ApprovedStatus");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "ProjectMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
