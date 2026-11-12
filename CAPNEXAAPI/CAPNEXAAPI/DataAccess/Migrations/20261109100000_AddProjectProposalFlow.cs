using CAPNEXAAPI.DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPNEXAAPI.DataAccess.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109100000_AddProjectProposalFlow")]
public partial class AddProjectProposalFlow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RejectionReason",
            table: "Projects",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "SubmittedAt",
            table: "Projects",
            type: "datetime2",
            nullable: true);

        migrationBuilder.DropIndex(
            name: "IX_Projects_TeamId",
            table: "Projects");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TeamId",
            table: "Projects",
            column: "TeamId",
            unique: true,
            filter: "[Status] <> 'Rejected'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Projects_TeamId",
            table: "Projects");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TeamId",
            table: "Projects",
            column: "TeamId");

        migrationBuilder.DropColumn(
            name: "RejectionReason",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "SubmittedAt",
            table: "Projects");
    }
}
