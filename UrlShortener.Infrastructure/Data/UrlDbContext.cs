using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Data
{
    // ملاحظة: يجب تمرير كلاس الـ User الخاص بك هنا ليعرف النظام أنك تستخدمه بدلاً من IdentityUser الافتراضي
    public class UrlDbContext : IdentityDbContext<User>
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }

        // لا نحتاج لتعريف DbSet<User> يدوياً هنا لأن IdentityDbContext<User> يقوم بذلك تلقائياً
        public DbSet<UrlMapping> UrlMappings { get; set; }
        public DbSet<ClickAnalytic> ClickAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // هام جداً: هذا السطر يحل مشكلة الـ Primary Key لجداول الـ Identity
            base.OnModelCreating(modelBuilder);

            // 1. Index على الـ ShortCode لأنه أكثر حاجة بنبحث عنها
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            // 2. علاقة الـ User بالـ UrlMappings
            modelBuilder.Entity<UrlMapping>()
                .HasOne(u => u.User)
                .WithMany(usr => usr.UrlMappings)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}