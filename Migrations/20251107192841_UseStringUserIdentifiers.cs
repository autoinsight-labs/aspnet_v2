using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet.Migrations
{
    /// <inheritdoc />
    public partial class UseStringUserIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "yards",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "yard_employees",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "inviter_id",
                table: "employee_invites",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                table: "yards",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "yard_employees",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<Guid>(
                name: "inviter_id",
                table: "employee_invites",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);
        }
    }
}
