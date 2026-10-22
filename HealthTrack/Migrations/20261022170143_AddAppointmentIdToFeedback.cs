using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentIdToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "appointmentId",
                table: "feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_appointmentId",
                table: "feedbacks",
                column: "appointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_feedbacks_appointments_appointmentId",
                table: "feedbacks",
                column: "appointmentId",
                principalTable: "appointments",
                principalColumn: "appointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedbacks_appointments_appointmentId",
                table: "feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_feedbacks_appointmentId",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "appointmentId",
                table: "feedbacks");
        }
    }
}
