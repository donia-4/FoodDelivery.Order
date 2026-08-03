using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Carts;

namespace Order.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CartId).IsRequired();
        builder.HasIndex(ci => ci.CartId);

        builder.Property(ci => ci.MenuItemId).IsRequired();
        builder.Property(ci => ci.RestaurantId).IsRequired();
        builder.Property(ci => ci.Quantity).IsRequired();
        builder.Property(ci => ci.UnitPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(ci => ci.Notes).HasMaxLength(500);

        // No navigation back to Cart — prevents cycles
    }
}