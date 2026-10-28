
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrudOperation.Migrations
{
    public partial class AddUserIsActive : Migration
    {
        
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add replacement indexes first, so foreign keys retain
    // a usable index on their leading columns.
    migrationBuilder.CreateIndex(
        name: "IX_TeacherAssignments_TeacherId_SubjectId",
        table: "TeacherAssignments",
        columns: new[] { "TeacherId", "SubjectId" },
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_StudentEnrollments_StudentId_ClassId",
        table: "StudentEnrollments",
        columns: new[] { "StudentId", "ClassId" },
        unique: true);

    // Drop old indexes only after creating replacements.
    migrationBuilder.DropIndex(
        name: "IX_TeacherAssignments_TeacherId",
        table: "TeacherAssignments");

    migrationBuilder.DropIndex(
        name: "IX_StudentEnrollments_StudentId",
        table: "StudentEnrollments");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Restore the original indexes.
    migrationBuilder.CreateIndex(
        name: "IX_TeacherAssignments_TeacherId",
        table: "TeacherAssignments",
        column: "TeacherId");

    migrationBuilder.CreateIndex(
        name: "IX_StudentEnrollments_StudentId",
        table: "StudentEnrollments",
        column: "StudentId");

    // Remove the composite indexes.
    migrationBuilder.DropIndex(
        name: "IX_TeacherAssignments_TeacherId_SubjectId",
        table: "TeacherAssignments");

    migrationBuilder.DropIndex(
        name: "IX_StudentEnrollments_StudentId_ClassId",
        table: "StudentEnrollments");
}

    }
}
