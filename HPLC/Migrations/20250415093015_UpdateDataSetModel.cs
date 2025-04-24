using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPLC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataSetModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Last_Used",
                table: "DataSet",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Last_Used",
                table: "DataSet");
        }
    }
}
