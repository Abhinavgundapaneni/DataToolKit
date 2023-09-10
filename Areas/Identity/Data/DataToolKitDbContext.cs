using DataToolKit.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataToolKit.Data;

public class DataToolKitDbContext : IdentityDbContext<DataToolKitUser>
{
    public DataToolKitDbContext(DbContextOptions<DataToolKitDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration( new DataToolKitUserEntityConfiguration());
    }
}

public class DataToolKitUserEntityConfiguration : IEntityTypeConfiguration<DataToolKitUser>
{
    public void Configure(EntityTypeBuilder<DataToolKitUser> builder)
    {
        builder.Property(u => u.firstName).HasMaxLength(255);
        builder.Property(u => u.lastName).HasMaxLength(255);
    }
}
