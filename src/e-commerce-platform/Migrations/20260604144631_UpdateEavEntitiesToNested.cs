using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_commerce_platform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEavEntitiesToNested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Users_MerchantId",
                table: "Attributes");

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_MerchantId",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Attributes");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "AttributeValues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Attributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Attributes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_ProductId",
                table: "Attributes",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Products_ProductId",
                table: "Attributes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Products_ProductId",
                table: "Attributes");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_ProductId",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "AttributeValues");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Attributes");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "Attributes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttributeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => new { x.ProductId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_ProductAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAttributes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_MerchantId",
                table: "Attributes",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeId",
                table: "ProductAttributes",
                column: "AttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Users_MerchantId",
                table: "Attributes",
                column: "MerchantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
