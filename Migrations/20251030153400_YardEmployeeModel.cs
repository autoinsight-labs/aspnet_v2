using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_v2.Migrations
{
    /// <inheritdoc />
    public partial class YardEmployeeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "admin,member")
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i");

            migrationBuilder.CreateTable(
                name: "yard_employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<int>(type: "employee_role", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    yard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_yard_employees", x => x.id);
                    table.ForeignKey(
                        name: "fk_yard_employees_yards_yard_id",
                        column: x => x.yard_id,
                        principalTable: "yards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_yard_employees_yard_id",
                table: "yard_employees",
                column: "yard_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "yard_employees");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:employee_role", "admin,member")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i");
        }
    }
}
