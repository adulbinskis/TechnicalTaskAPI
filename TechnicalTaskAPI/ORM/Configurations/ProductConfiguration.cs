using TechnicalTaskAPI.ORM.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace TechnicalTaskAPI.ORM.Configurations
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Quantity)
                   .IsRequired();

            builder.Property(p => p.PricePerUnit)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
        }
    }
}
