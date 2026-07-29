using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Orders;

namespace Order.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(osh => osh.Id);

        builder.Property(osh => osh.OrderId)
            .IsRequired();

        builder.HasIndex(osh => osh.OrderId);

        builder.Property(osh => osh.OldStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(osh => osh.NewStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(osh => osh.ChangedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(osh => osh.ChangedDate)
            .IsRequired();

        // No navigation back to Order — prevents cycles
        // Delete is handled by OrderConfiguration Cascade
    }
}