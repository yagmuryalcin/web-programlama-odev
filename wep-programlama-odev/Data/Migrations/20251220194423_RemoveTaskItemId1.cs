using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wep_programlama_odev.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskItemId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_TaskItems_TaskItemId1",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_TaskItemId1",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "TaskItemId1",
                table: "TaskComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskItemId1",
                table: "TaskComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskItemId1",
                table: "TaskComments",
                column: "TaskItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_TaskItems_TaskItemId1",
                table: "TaskComments",
                column: "TaskItemId1",
                principalTable: "TaskItems",
                principalColumn: "Id");
        }
    }
}
