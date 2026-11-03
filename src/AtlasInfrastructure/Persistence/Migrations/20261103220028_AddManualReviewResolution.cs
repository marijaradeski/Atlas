using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasInfrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManualReviewResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResolutionOutcome",
                table: "RequirementEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionReason",
                table: "RequirementEvaluations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAtUtc",
                table: "RequirementEvaluations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedByStaffId",
                table: "RequirementEvaluations",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolutionOutcome",
                table: "RequirementEvaluations");

            migrationBuilder.DropColumn(
                name: "ResolutionReason",
                table: "RequirementEvaluations");

            migrationBuilder.DropColumn(
                name: "ResolvedAtUtc",
                table: "RequirementEvaluations");

            migrationBuilder.DropColumn(
                name: "ResolvedByStaffId",
                table: "RequirementEvaluations");
        }
    }
}
