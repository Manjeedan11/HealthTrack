using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutAndMealLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mealLogs",
                columns: table => new
                {
                    mealLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    mealId = table.Column<int>(type: "int", nullable: true),
                    dietPlanId = table.Column<int>(type: "int", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    caloriesConsumed = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mealLogs", x => x.mealLogId);
                    table.ForeignKey(
                        name: "FK_mealLogs_Clients_clientId",
                        column: x => x.clientId,
                        principalTable: "Clients",
                        principalColumn: "clientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mealLogs_dietPlans_dietPlanId",
                        column: x => x.dietPlanId,
                        principalTable: "dietPlans",
                        principalColumn: "dietId");
                    table.ForeignKey(
                        name: "FK_mealLogs_meals_mealId",
                        column: x => x.mealId,
                        principalTable: "meals",
                        principalColumn: "mealId");
                });

            migrationBuilder.CreateTable(
                name: "workoutLogs",
                columns: table => new
                {
                    workoutLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    exerciseId = table.Column<int>(type: "int", nullable: true),
                    workoutPlanId = table.Column<int>(type: "int", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    durationMinutes = table.Column<int>(type: "int", nullable: false),
                    caloriesBurned = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workoutLogs", x => x.workoutLogId);
                    table.ForeignKey(
                        name: "FK_workoutLogs_Clients_clientId",
                        column: x => x.clientId,
                        principalTable: "Clients",
                        principalColumn: "clientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workoutLogs_exercises_exerciseId",
                        column: x => x.exerciseId,
                        principalTable: "exercises",
                        principalColumn: "exerciseId");
                    table.ForeignKey(
                        name: "FK_workoutLogs_workoutPlans_workoutPlanId",
                        column: x => x.workoutPlanId,
                        principalTable: "workoutPlans",
                        principalColumn: "planId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_mealLogs_clientId",
                table: "mealLogs",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_mealLogs_dietPlanId",
                table: "mealLogs",
                column: "dietPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_mealLogs_mealId",
                table: "mealLogs",
                column: "mealId");

            migrationBuilder.CreateIndex(
                name: "IX_workoutLogs_clientId",
                table: "workoutLogs",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_workoutLogs_exerciseId",
                table: "workoutLogs",
                column: "exerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_workoutLogs_workoutPlanId",
                table: "workoutLogs",
                column: "workoutPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mealLogs");

            migrationBuilder.DropTable(
                name: "workoutLogs");
        }
    }
}
