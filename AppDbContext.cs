using Microsoft.EntityFrameworkCore;
using SideProject0303.Models;

namespace SideProject0303;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Couple> Couples => Set<Couple>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Couple.Code 유니크
        modelBuilder.Entity<Couple>()
            .HasIndex(x => x.Code)
            .IsUnique();

        // User - Couple 관계 (1:N)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Couple)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CoupleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event - Couple 관계 (1:N)
        modelBuilder.Entity<CalendarEvent>()
            .HasOne(e => e.Couple)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CoupleId)
            .OnDelete(DeleteBehavior.Cascade);

        // 조회 최적화 인덱스: 커플+날짜, 커플+타입+날짜
        modelBuilder.Entity<CalendarEvent>()
            .HasIndex(e => new { e.CoupleId, e.StartDate });

        modelBuilder.Entity<CalendarEvent>()
            .HasIndex(e => new { e.CoupleId, e.Type, e.StartDate });
    }
}
