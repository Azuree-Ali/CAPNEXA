using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPNEXAAPI.DataAccess.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109090000_OneTeamPerStudent")]
public partial class OneTeamPerStudent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMembers_StudentId",
            table: "TeamMembers");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_StudentId",
            table: "TeamMembers",
            column: "StudentId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMembers_StudentId",
            table: "TeamMembers");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_StudentId",
            table: "TeamMembers",
            column: "StudentId");
    }
}
