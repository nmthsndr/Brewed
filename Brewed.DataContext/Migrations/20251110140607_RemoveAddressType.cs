using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewed.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressType",
                table: "Addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressType",
                table: "Addresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Shipping");
        }
    }
}