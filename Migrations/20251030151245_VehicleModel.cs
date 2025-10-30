using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_v2.Migrations
{
    /// <inheritdoc />
    public partial class VehicleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i");

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "text", nullable: false),
                    model = table.Column<int>(type: "vehicle_model", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i");
        }
    }
}
