using System;
using AutoInsight.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_v2.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "yard_vehicles");

            migrationBuilder.AddColumn<Guid>(
                name: "assignee_id",
                table: "vehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "entered_at",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "left_at",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<VehicleStatus>(
                name: "status",
                table: "vehicles",
                type: "vehicle_status",
                nullable: false,
                defaultValue: VehicleStatus.Scheduled);

            migrationBuilder.AddColumn<Guid>(
                name: "yard_id",
                table: "vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_assignee_id",
                table: "vehicles",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_yard_id",
                table: "vehicles",
                column: "yard_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vehicles_yard_employees_assignee_id",
                table: "vehicles",
                column: "assignee_id",
                principalTable: "yard_employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_vehicles_yards_yard_id",
                table: "vehicles",
                column: "yard_id",
                principalTable: "yards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vehicles_yard_employees_assignee_id",
                table: "vehicles");

            migrationBuilder.DropForeignKey(
                name: "fk_vehicles_yards_yard_id",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_assignee_id",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_yard_id",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "assignee_id",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "entered_at",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "left_at",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "status",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "yard_id",
                table: "vehicles");

            migrationBuilder.CreateTable(
                name: "yard_vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    yard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    left_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "vehicle_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_yard_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "fk_yard_vehicles_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_yard_vehicles_yards_yard_id",
                        column: x => x.yard_id,
                        principalTable: "yards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_yard_vehicles_vehicle_id",
                table: "yard_vehicles",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_yard_vehicles_yard_id",
                table: "yard_vehicles",
                column: "yard_id");
        }
    }
}
