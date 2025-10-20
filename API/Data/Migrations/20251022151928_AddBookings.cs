using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreetAddress = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Province = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressId = table.Column<int>(type: "INTEGER", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppointmentSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentSlotId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PatientEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AppointmentSlots_AppointmentSlotId",
                        column: x => x.AppointmentSlotId,
                        principalTable: "AppointmentSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_ClinicId_StartTime",
                table: "AppointmentSlots",
                columns: new[] { "ClinicId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppointmentSlotId",
                table: "Bookings",
                column: "AppointmentSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClinicId",
                table: "Bookings",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_AddressId",
                table: "Clinics",
                column: "AddressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "AppointmentSlots");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "Address");
        }
    }
}
