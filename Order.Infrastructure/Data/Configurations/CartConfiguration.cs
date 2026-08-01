using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Carts;

namespace Order.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerId).IsRequired();
        builder.HasIndex(c => c.CustomerId);

        builder.Property(c => c.RestaurantId).IsRequired();
        builder.HasIndex(c => c.RestaurantId);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.LastModifiedUtc).IsRequired();
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}