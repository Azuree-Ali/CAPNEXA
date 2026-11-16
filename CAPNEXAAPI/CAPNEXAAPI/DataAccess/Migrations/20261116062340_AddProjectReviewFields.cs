using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPNEXAAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_TeamId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TeamId",
                table: "Projects",
                column: "TeamId",
                unique: true,
                filter: "[Status] <> 'Rejected'");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "Projects",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ReviewedByUserId",
                table: "Projects",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status_SubmittedAt",
                table: "Projects",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_ReviewedByUserId",
                table: "Projects",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status_SubmittedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TeamId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TeamId",
                table: "Projects",
                column: "TeamId",
                unique: true,
                filter: "[Status] <> 'Rejected'");
        }
    }
}
