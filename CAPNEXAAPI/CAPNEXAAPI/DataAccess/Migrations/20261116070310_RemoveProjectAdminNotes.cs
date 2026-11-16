using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPNEXAAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectAdminNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
