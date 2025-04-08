﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorTemplate.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerCountToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkerCount",
                table: "Companies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 1,
                column: "WorkerCount",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 2,
                column: "WorkerCount",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 3,
                column: "WorkerCount",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkerCount",
                table: "Companies");
        }
    }
}
