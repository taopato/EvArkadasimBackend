using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class LedgerLineConfiguration : IEntityTypeConfiguration<LedgerLine>
    {
        public void Configure(EntityTypeBuilder<LedgerLine> b)
        {
            b.ToTable("LedgerLines");
            b.HasKey(x => x.Id);

            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.PostDate).HasDefaultValueSql("sysutcdatetime()");
            b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()");

            // Navigation YOK — doğrudan FK
            b.HasOne<Expense>()
             .WithMany()
             .HasForeignKey(x => x.ExpenseId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.ExpenseId).HasDatabaseName("IX_Ledger_Expense");
            b.HasIndex(x => new { x.HouseId, x.PostDate }).HasDatabaseName("IX_Ledger_House_Post");
            b.HasIndex(x => new { x.HouseId, x.FromUserId, x.ToUserId, x.PostDate })
             .HasDatabaseName("IX_Ledger_House_FromTo");
        }
    }
}
