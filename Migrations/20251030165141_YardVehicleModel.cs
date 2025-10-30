using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_v2.Migrations
{
    /// <inheritdoc />
    public partial class YardVehicleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "admin,member")
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .Annotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:employee_role", "admin,member")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i");

            migrationBuilder.CreateTable(
                name: "yard_vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "vehicle_status", nullable: false),
                    entered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    left_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    yard_id = table.Column<Guid>(type: "uuid", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "yard_vehicles");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "admin,member")
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:employee_role", "admin,member")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled");
        }
    }
}
