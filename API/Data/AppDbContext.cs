using API.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();

    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
          new IdentityRole
          {
            Id = "f0b9d7c0-6d2e-4b0a-9c3e-3d9e1a7f0001",
            Name = "User",
            NormalizedName = "USER",
            ConcurrencyStamp = "4f2c6b77-2a73-4a32-9e0a-111111111111"
          },
          new IdentityRole
          {
            Id = "f0b9d7c0-6d2e-4b0a-9c3e-3d9e1a7f0002",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "4f2c6b77-2a73-4a32-9e0a-222222222222"
          }
        );

        builder.Entity<Clinic>()
            .HasMany(clinic => clinic.AppointmentSlots)
            .WithOne(slot => slot.Clinic!)
            .HasForeignKey(slot => slot.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Clinic>()
            .HasMany(clinic => clinic.Bookings)
            .WithOne(booking => booking.Clinic!)
            .HasForeignKey(booking => booking.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AppointmentSlot>()
            .HasOne(slot => slot.Booking)
            .WithOne(booking => booking.AppointmentSlot!)
            .HasForeignKey<Booking>(booking => booking.AppointmentSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AppointmentSlot>()
            .HasIndex(slot => new { slot.ClinicId, slot.StartTime })
            .IsUnique();

        builder.Entity<Booking>()
            .HasIndex(booking => booking.AppointmentSlotId)
            .IsUnique();

        builder.Entity<Booking>()
            .Property(booking => booking.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
