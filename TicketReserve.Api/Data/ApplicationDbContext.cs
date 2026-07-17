using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TicketReserve.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // IdentityDbContext'in AspNetUsers/AspNetRoles vb. için kendi model kurulumunu yapması gerekiyor —
        // bunu atlarsan Identity tabloları yanlış/eksik konfigüre edilir.
        base.OnModelCreating(builder);

        builder.Entity<Seat>()
            .HasIndex(seat => new { seat.EventId, seat.Row, seat.Number })
            .IsUnique()
            // Soft-delete edilmiş bir koltuk, aynı Row/Number ile yeniden oluşturulabilsin diye
            // unique index sadece silinmemiş satırları kapsıyor (Postgres partial index).
            .HasFilter("\"IsDeleted\" = false");

        builder.Entity<Event>()
            .HasMany(e => e.Seats)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete: silinmiş kayıtlar hiçbir sorguda (elle filtre eklemeden) görünmesin.
        builder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Seat>().HasQueryFilter(s => !s.IsDeleted);

        // Bir koltuk en fazla bir kez satılabilir — DB seviyesinde son savunma hattı.
        builder.Entity<Reservation>()
            .HasIndex(r => r.SeatId)
            .IsUnique();

        // UseXminAsConcurrencyToken() Npgsql provider'ından 7.0'da kaldırıldı; artık standart EF Core
        // IsRowVersion() API'si kullanılıyor. Provider bunu arka planda Postgres'in "xmin" sistem
        // kolonuna eşliyor — Seat.Version elle set edilmiyor, satır her değiştiğinde Postgres kendisi günceller.
        builder.Entity<Seat>()
            .Property(s => s.Version)
            .IsRowVersion();
    }
}
