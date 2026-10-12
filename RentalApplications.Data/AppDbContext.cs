using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<RentalApplication> RentalApplications => Set<RentalApplication>();
    public DbSet<ResidenceHistory> ResidenceHistories => Set<ResidenceHistory>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Property>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200);
            entity.Property(p => p.StreetAddress).HasMaxLength(300);
            entity.Property(p => p.City).HasMaxLength(100);
            entity.Property(p => p.State).HasMaxLength(50);
            entity.Property(p => p.PostalCode).HasMaxLength(20);
        });

        builder.Entity<UnitType>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(100);
        });

        builder.Entity<Unit>(entity =>
        {
            entity.Property(u => u.UnitNumber).HasMaxLength(50);
            entity.Property(u => u.MonthlyRent).HasPrecision(18, 2);
            entity.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();
        });

        builder.Entity<RentalApplication>(entity =>
        {
            entity.Property(a => a.ApplicantName).HasMaxLength(200);
            entity.Property(a => a.Phone).HasMaxLength(50);
            entity.Property(a => a.Email).HasMaxLength(256);
            entity.Property(a => a.CurrentStreetAddress).HasMaxLength(300);
            entity.Property(a => a.CurrentCity).HasMaxLength(100);
            entity.Property(a => a.CurrentState).HasMaxLength(50);
            entity.Property(a => a.CurrentPostalCode).HasMaxLength(20);
            entity.HasOne(a => a.Lease)
                .WithOne(l => l.RentalApplication)
                .HasForeignKey<Lease>(l => l.RentalApplicationId);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.ApplicantUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ResidenceHistory>(entity =>
        {
            entity.Property(r => r.StreetAddress).HasMaxLength(300);
            entity.Property(r => r.City).HasMaxLength(100);
            entity.Property(r => r.State).HasMaxLength(50);
            entity.Property(r => r.PostalCode).HasMaxLength(20);
            entity.Property(r => r.LandlordName).HasMaxLength(200);
            entity.Property(r => r.LandlordPhone).HasMaxLength(50);
        });

        builder.Entity<Lease>(entity =>
        {
            entity.HasIndex(l => l.UnitId);
            entity.HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.Property(h => h.Comment).HasMaxLength(2000);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
