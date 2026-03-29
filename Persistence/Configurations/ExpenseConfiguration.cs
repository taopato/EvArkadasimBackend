using Domain.Entities;
using Domain.Enums; // <= önemli
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> b)
        {
            b.ToTable("Expenses");

            b.Property(e => e.Type).HasConversion<byte>();
            b.Property(e => e.Category).HasConversion<byte>();
            b.Property(e => e.VisibilityMode).HasConversion<byte>();
            b.Property(e => e.PeriodMonth).HasMaxLength(7);

            b.HasIndex(e => new { e.HouseId, e.PostDate }).HasDatabaseName("IX_Expenses_House_Post");
            b.HasIndex(e => new { e.HouseId, e.PeriodMonth }).HasDatabaseName("IX_Expenses_Period");
            b.HasIndex(e => e.RecurrenceBatchKey).HasDatabaseName("IX_Expenses_Batch");
        }
    }
}
