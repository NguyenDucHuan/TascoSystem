using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasco.ProjectService.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "ProjectMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ProjectMembers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ProjectMembers");
        }
    }
}
