using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasInfrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationStaffAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EvaluatedByStaffId",
                table: "RequirementEvaluations",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluatedByStaffId",
                table: "RequirementEvaluations");
        }
    }
}
