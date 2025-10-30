using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_v2.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeInviteModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "admin,member")
                .Annotation("Npgsql:Enum:invite_status", "pending,accepted,rejected")
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .Annotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:employee_role", "admin,member")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled");

            migrationBuilder.CreateTable(
                name: "employee_invites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "employee_role", nullable: false),
                    status = table.Column<int>(type: "invite_status", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inviter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    yard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_invites", x => x.id);
                    table.ForeignKey(
                        name: "fk_employee_invites_yards_yard_id",
                        column: x => x.yard_id,
                        principalTable: "yards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_employee_invites_yard_id",
                table: "employee_invites",
                column: "yard_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_invites");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "admin,member")
                .Annotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .Annotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:employee_role", "admin,member")
                .OldAnnotation("Npgsql:Enum:invite_status", "pending,accepted,rejected")
                .OldAnnotation("Npgsql:Enum:vehicle_model", "mottu_sport110i,mottue,honda_pop110i,tvs_sport110i")
                .OldAnnotation("Npgsql:Enum:vehicle_status", "scheduled,waiting,on_service,finished,cancelled");
        }
    }
}
