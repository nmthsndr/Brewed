using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewed.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCouponTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to Coupons table
            migrationBuilder.AddColumn<int>(
                name: "MaxUsageCount",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "Coupons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Create UserCoupons table
            migrationBuilder.CreateTable(
                name: "UserCoupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_UserId",
                table: "UserCoupons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_CouponId",
                table: "UserCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_OrderId",
                table: "UserCoupons",
                column: "OrderId");

            // Unique constraint: egy felhasználó egy kupont csak egyszer kaphat meg
            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_UserId_CouponId",
                table: "UserCoupons",
                columns: new[] { "UserId", "CouponId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCoupons");

            migrationBuilder.DropColumn(
                name: "MaxUsageCount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "Coupons");
        }
    }
}