using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication10.Models;

public partial class CinemaTicketContext : DbContext
{
    public CinemaTicketContext()
    {
    }

    public CinemaTicketContext(DbContextOptions<CinemaTicketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Cinema> Cinemas { get; set; }

    public virtual DbSet<CinemaRoom> CinemaRooms { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatLock> SeatLocks { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-2JHP82JS\\BVS;Initial Catalog=CinemaTicket;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__BOOKING__73951ACD5E050FDC");

            entity.ToTable("BOOKING");

            entity.Property(e => e.BookingId)
                .ValueGeneratedNever()
                .HasColumnName("BookingID");
            entity.Property(e => e.BookingStatus).HasMaxLength(50);
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.ShowtimeId).HasColumnName("ShowtimeID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK_BOOKING_PROMOTION");

            entity.HasOne(d => d.Showtime).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ShowtimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOOKING_SHOWTIME");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOOKING_USER");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => e.BookingDetailId).HasName("PK__BOOKING___8136D47AC7B4B94B");

            entity.ToTable("BOOKING_DETAIL");

            entity.HasIndex(e => new { e.BookingId, e.SeatId }, "UQ__BOOKING___50846BF1C0CAF50F").IsUnique();

            entity.Property(e => e.BookingDetailId)
                .ValueGeneratedNever()
                .HasColumnName("BookingDetailID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SeatId).HasColumnName("SeatID");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOOKING_DETAIL_BOOKING");

            entity.HasOne(d => d.Seat).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOOKING_DETAIL_SEAT");
        });

        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.HasKey(e => e.CinemaId).HasName("PK__CINEMA__59C9262692A144A9");

            entity.ToTable("CINEMA");

            entity.HasIndex(e => e.CinemaName, "UQ__CINEMA__5F9CB2D25F178657").IsUnique();

            entity.Property(e => e.CinemaId)
                .ValueGeneratedNever()
                .HasColumnName("CinemaID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CinemaName).HasMaxLength(100);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CinemaRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__CINEMA_R__32863919D5C25568");

            entity.ToTable("CINEMA_ROOM");

            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("RoomID");
            entity.Property(e => e.CinemaId).HasColumnName("CinemaID");
            entity.Property(e => e.RoomName).HasMaxLength(100);
            entity.Property(e => e.RoomType).HasMaxLength(50);

            entity.HasOne(d => d.Cinema).WithMany(p => p.CinemaRooms)
                .HasForeignKey(d => d.CinemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CINEMA_ROOM_CINEMA");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("PK__MOVIE__4BD2943A2E68EB78");

            entity.ToTable("MOVIE");

            entity.Property(e => e.MovieId)
                .ValueGeneratedNever()
                .HasColumnName("MovieID");
            entity.Property(e => e.AgeRating).HasMaxLength(20);
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.Poster).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PAYMENT__9B556A58005EB9CD");

            entity.ToTable("PAYMENT");

            entity.HasIndex(e => e.BookingId, "UQ__PAYMENT__73951ACCF6C63A63").IsUnique();

            entity.HasIndex(e => e.TransactionCode, "UQ__PAYMENT__D85E7026D7635674").IsUnique();

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Booking).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAYMENT_BOOKING");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__PROMOTIO__52C42F2F8C1C1794");

            entity.ToTable("PROMOTION");

            entity.HasIndex(e => e.PromotionCode, "UQ__PROMOTIO__A617E4B6175BB5DE").IsUnique();

            entity.Property(e => e.PromotionId)
                .ValueGeneratedNever()
                .HasColumnName("PromotionID");
            entity.Property(e => e.DiscountType).HasMaxLength(50);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromotionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLE__8AFACE3AD8F78A65");

            entity.ToTable("ROLE");

            entity.HasIndex(e => e.RoleName, "UQ__ROLE__8A2B616064952D8D").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__SEAT__311713D3B6CD73F8");

            entity.ToTable("SEAT");

            entity.Property(e => e.SeatId)
                .ValueGeneratedNever()
                .HasColumnName("SeatID");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.RowNumber).HasMaxLength(10);
            entity.Property(e => e.SeatNumber).HasMaxLength(10);
            entity.Property(e => e.SeatType).HasMaxLength(50);

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SEAT_ROOM");
        });

        modelBuilder.Entity<SeatLock>(entity =>
        {
            entity.HasKey(e => e.SeatLockId).HasName("PK__SEAT_LOC__BA5264CCEC716098");

            entity.ToTable("SEAT_LOCK");

            entity.Property(e => e.SeatLockId)
                .ValueGeneratedNever()
                .HasColumnName("SeatLockID");
            entity.Property(e => e.LockStatus).HasMaxLength(50);
            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.ShowtimeId).HasColumnName("ShowtimeID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Seat).WithMany(p => p.SeatLocks)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SEAT_LOCK_SEAT");

            entity.HasOne(d => d.Showtime).WithMany(p => p.SeatLocks)
                .HasForeignKey(d => d.ShowtimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SEAT_LOCK_SHOWTIME");

            entity.HasOne(d => d.User).WithMany(p => p.SeatLocks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SEAT_LOCK_USER");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.ShowtimeId).HasName("PK__SHOWTIME__32D31FC05E41EBD4");

            entity.ToTable("SHOWTIME");

            entity.Property(e => e.ShowtimeId)
                .ValueGeneratedNever()
                .HasColumnName("ShowtimeID");
            entity.Property(e => e.MovieId).HasColumnName("MovieID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SHOWTIME_MOVIE");

            entity.HasOne(d => d.Room).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SHOWTIME_ROOM");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__TICKET__712CC6277A09AEAC");

            entity.ToTable("TICKET");

            entity.HasIndex(e => e.TicketCode, "UQ__TICKET__598CF7A31914E363").IsUnique();

            entity.HasIndex(e => e.BookingDetailId, "UQ__TICKET__8136D47BD7A12596").IsUnique();

            entity.Property(e => e.TicketId)
                .ValueGeneratedNever()
                .HasColumnName("TicketID");
            entity.Property(e => e.BookingDetailId).HasColumnName("BookingDetailID");
            entity.Property(e => e.Qrcode)
                .HasMaxLength(500)
                .HasColumnName("QRCode");
            entity.Property(e => e.TicketCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TicketStatus).HasMaxLength(50);

            entity.HasOne(d => d.BookingDetail).WithOne(p => p.Ticket)
                .HasForeignKey<Ticket>(d => d.BookingDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TICKET_BOOKING_DETAIL");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USER__1788CCAC902BD3BF");

            entity.ToTable("USER");

            entity.HasIndex(e => e.Phone, "UQ__USER__5C7E359E8531957C").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__USER__A9D1053496537362").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
