using CloudStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.Infrastructure.Persistence;

public class CloudStorageDbContext : DbContext
{
    public CloudStorageDbContext(
        DbContextOptions<CloudStorageDbContext> options)
        : base(options)
    {
    }

    public DbSet<StoredFile> StoredFiles { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.ToTable("StoredFiles");

            entity.Property(e => e.Id);
            
            entity.Property(e => e.OriginalName)
                .HasColumnName("OriginalName")
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.ContentType)
                .HasColumnName("ContentType")
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.SizeBytes)
                .HasColumnName("SizeBytes")
                .IsRequired();
            
            entity.Property(e => e.StoragePath)
                .HasColumnName("StoragePath")
                .IsRequired()
                .HasMaxLength(2048);
            
            entity.Property(e => e.Sha256Hash)
                .HasColumnName("Sha256Hash")
                .IsRequired()
                .HasMaxLength(64)
                .IsFixedLength(); 
            
            entity.Property(e => e.UploadedAt)
                .HasColumnName("UploadedAt")
                .IsRequired()
                .HasColumnType("timestamp with time zone"); 
        });
    }
}