using Microsoft.EntityFrameworkCore;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Parent>()
            .HasIndex(p => p.Email).IsUnique(false);

        b.Entity<Child>()
            .HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<AttendanceLog>()
            .HasOne(a => a.Child)
            .WithMany()
            .HasForeignKey(a => a.ChildId);

        b.Entity<AttendanceLog>()
            .HasOne<StaffUser>(a => a.HandledBy)
            .WithMany()
            .HasForeignKey(a => a.HandledByUserId);
    }
}