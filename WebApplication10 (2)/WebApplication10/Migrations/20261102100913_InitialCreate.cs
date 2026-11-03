using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication10.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CINEMA",
                columns: table => new
                {
                    CinemaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CinemaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CINEMA__59C9262692A144A9", x => x.CinemaID);
                });

            migrationBuilder.CreateTable(
                name: "MOVIE",
                columns: table => new
                {
                    MovieID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Poster = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AgeRating = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MOVIE__4BD2943A2E68EB78", x => x.MovieID);
                });

            migrationBuilder.CreateTable(
                name: "PROMOTION",
                columns: table => new
                {
                    PromotionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PROMOTIO__52C42F2F8C1C1794", x => x.PromotionID);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ROLE__8AFACE3AD8F78A65", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "CINEMA_ROOM",
                columns: table => new
                {
                    RoomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CinemaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CINEMA_R__32863919D5C25568", x => x.RoomID);
                    table.ForeignKey(
                        name: "FK_CINEMA_ROOM_CINEMA",
                        column: x => x.CinemaID,
                        principalTable: "CINEMA",
                        principalColumn: "CinemaID");
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USER__1788CCAC902BD3BF", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_USER_ROLE",
                        column: x => x.RoleID,
                        principalTable: "ROLE",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "SEAT",
                columns: table => new
                {
                    SeatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeatNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RowNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SeatType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SEAT__311713D3B6CD73F8", x => x.SeatID);
                    table.ForeignKey(
                        name: "FK_SEAT_ROOM",
                        column: x => x.RoomID,
                        principalTable: "CINEMA_ROOM",
                        principalColumn: "RoomID");
                });

            migrationBuilder.CreateTable(
                name: "SHOWTIME",
                columns: table => new
                {
                    ShowtimeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MovieID = table.Column<int>(type: "int", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SHOWTIME__32D31FC05E41EBD4", x => x.ShowtimeID);
                    table.ForeignKey(
                        name: "FK_SHOWTIME_MOVIE",
                        column: x => x.MovieID,
                        principalTable: "MOVIE",
                        principalColumn: "MovieID");
                    table.ForeignKey(
                        name: "FK_SHOWTIME_ROOM",
                        column: x => x.RoomID,
                        principalTable: "CINEMA_ROOM",
                        principalColumn: "RoomID");
                });

            migrationBuilder.CreateTable(
                name: "BOOKING",
                columns: table => new
                {
                    BookingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ShowtimeID = table.Column<int>(type: "int", nullable: false),
                    PromotionID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BOOKING__73951ACD5E050FDC", x => x.BookingID);
                    table.ForeignKey(
                        name: "FK_BOOKING_PROMOTION",
                        column: x => x.PromotionID,
                        principalTable: "PROMOTION",
                        principalColumn: "PromotionID");
                    table.ForeignKey(
                        name: "FK_BOOKING_SHOWTIME",
                        column: x => x.ShowtimeID,
                        principalTable: "SHOWTIME",
                        principalColumn: "ShowtimeID");
                    table.ForeignKey(
                        name: "FK_BOOKING_USER",
                        column: x => x.UserID,
                        principalTable: "USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SEAT_LOCK",
                columns: table => new
                {
                    SeatLockID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SeatID = table.Column<int>(type: "int", nullable: false),
                    ShowtimeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SEAT_LOC__BA5264CCEC716098", x => x.SeatLockID);
                    table.ForeignKey(
                        name: "FK_SEAT_LOCK_SEAT",
                        column: x => x.SeatID,
                        principalTable: "SEAT",
                        principalColumn: "SeatID");
                    table.ForeignKey(
                        name: "FK_SEAT_LOCK_SHOWTIME",
                        column: x => x.ShowtimeID,
                        principalTable: "SHOWTIME",
                        principalColumn: "ShowtimeID");
                    table.ForeignKey(
                        name: "FK_SEAT_LOCK_USER",
                        column: x => x.UserID,
                        principalTable: "USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "BOOKING_DETAIL",
                columns: table => new
                {
                    BookingDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingID = table.Column<int>(type: "int", nullable: false),
                    SeatID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BOOKING___8136D47AC7B4B94B", x => x.BookingDetailID);
                    table.ForeignKey(
                        name: "FK_BOOKING_DETAIL_BOOKING",
                        column: x => x.BookingID,
                        principalTable: "BOOKING",
                        principalColumn: "BookingID");
                    table.ForeignKey(
                        name: "FK_BOOKING_DETAIL_SEAT",
                        column: x => x.SeatID,
                        principalTable: "SEAT",
                        principalColumn: "SeatID");
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT",
                columns: table => new
                {
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    BookingID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PAYMENT__9B556A58005EB9CD", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOKING",
                        column: x => x.BookingID,
                        principalTable: "BOOKING",
                        principalColumn: "BookingID");
                });

            migrationBuilder.CreateTable(
                name: "TICKET",
                columns: table => new
                {
                    TicketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookingDetailID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TICKET__712CC6277A09AEAC", x => x.TicketID);
                    table.ForeignKey(
                        name: "FK_TICKET_BOOKING_DETAIL",
                        column: x => x.BookingDetailID,
                        principalTable: "BOOKING_DETAIL",
                        principalColumn: "BookingDetailID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_PromotionID",
                table: "BOOKING",
                column: "PromotionID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_ShowtimeID",
                table: "BOOKING",
                column: "ShowtimeID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_UserID",
                table: "BOOKING",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_DETAIL_SeatID",
                table: "BOOKING_DETAIL",
                column: "SeatID");

            migrationBuilder.CreateIndex(
                name: "UQ__BOOKING___50846BF1C0CAF50F",
                table: "BOOKING_DETAIL",
                columns: new[] { "BookingID", "SeatID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__CINEMA__5F9CB2D25F178657",
                table: "CINEMA",
                column: "CinemaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CINEMA_ROOM_CinemaID",
                table: "CINEMA_ROOM",
                column: "CinemaID");

            migrationBuilder.CreateIndex(
                name: "UQ__PAYMENT__73951ACCF6C63A63",
                table: "PAYMENT",
                column: "BookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__PAYMENT__D85E7026D7635674",
                table: "PAYMENT",
                column: "TransactionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__PROMOTIO__A617E4B6175BB5DE",
                table: "PROMOTION",
                column: "PromotionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__ROLE__8A2B616064952D8D",
                table: "ROLE",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEAT_RoomID",
                table: "SEAT",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_SEAT_LOCK_SeatID",
                table: "SEAT_LOCK",
                column: "SeatID");

            migrationBuilder.CreateIndex(
                name: "IX_SEAT_LOCK_ShowtimeID",
                table: "SEAT_LOCK",
                column: "ShowtimeID");

            migrationBuilder.CreateIndex(
                name: "IX_SEAT_LOCK_UserID",
                table: "SEAT_LOCK",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SHOWTIME_MovieID",
                table: "SHOWTIME",
                column: "MovieID");

            migrationBuilder.CreateIndex(
                name: "IX_SHOWTIME_RoomID",
                table: "SHOWTIME",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "UQ__TICKET__598CF7A31914E363",
                table: "TICKET",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__TICKET__8136D47BD7A12596",
                table: "TICKET",
                column: "BookingDetailID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_RoleID",
                table: "USER",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UQ__USER__5C7E359E8531957C",
                table: "USER",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__USER__A9D1053496537362",
                table: "USER",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENT");

            migrationBuilder.DropTable(
                name: "SEAT_LOCK");

            migrationBuilder.DropTable(
                name: "TICKET");

            migrationBuilder.DropTable(
                name: "BOOKING_DETAIL");

            migrationBuilder.DropTable(
                name: "BOOKING");

            migrationBuilder.DropTable(
                name: "SEAT");

            migrationBuilder.DropTable(
                name: "PROMOTION");

            migrationBuilder.DropTable(
                name: "SHOWTIME");

            migrationBuilder.DropTable(
                name: "USER");

            migrationBuilder.DropTable(
                name: "MOVIE");

            migrationBuilder.DropTable(
                name: "CINEMA_ROOM");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.DropTable(
                name: "CINEMA");
        }
    }
}
