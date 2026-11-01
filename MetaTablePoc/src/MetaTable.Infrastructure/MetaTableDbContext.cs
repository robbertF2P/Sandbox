using MetaTable.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaTable.Infrastructure;

public sealed class MetaTableDbContext(DbContextOptions<MetaTableDbContext> options) : DbContext(options)
{
    public DbSet<MetaTableEntity> MetaTables => Set<MetaTableEntity>();

    public DbSet<MetaColumnEntity> MetaColumns => Set<MetaColumnEntity>();

    public DbSet<MetaRowEntity> MetaRows => Set<MetaRowEntity>();

    public DbSet<MetaTextCellEntity> MetaTextCells => Set<MetaTextCellEntity>();

    public DbSet<MetaIntegerCellEntity> MetaIntegerCells => Set<MetaIntegerCellEntity>();

    public DbSet<MetaAssociationCellEntity> MetaAssociationCells => Set<MetaAssociationCellEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MetaTableEntity>(entity =>
        {
            entity.ToTable("meta_tables");
            entity.HasKey(table => table.Id);
            entity.HasIndex(table => table.Key).IsUnique();
            entity.Property(table => table.Key).HasMaxLength(128).IsRequired();
            entity.Property(table => table.DisplayName).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<MetaColumnEntity>(entity =>
        {
            entity.ToTable("meta_columns");
            entity.HasKey(column => column.Id);
            entity.HasIndex(column => new { column.TableId, column.Name }).IsUnique();
            entity.Property(column => column.Name).HasMaxLength(128).IsRequired();
            entity.HasOne(column => column.Table)
                .WithMany()
                .HasForeignKey(column => column.TableId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(column => column.AssociationTargetTable)
                .WithMany()
                .HasForeignKey(column => column.AssociationTargetTableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MetaRowEntity>(entity =>
        {
            entity.ToTable("meta_rows");
            entity.HasKey(row => row.Id);
            entity.HasOne(row => row.Table)
                .WithMany()
                .HasForeignKey(row => row.TableId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetaTextCellEntity>(entity =>
        {
            entity.ToTable("meta_text_cells");
            entity.HasKey(cell => new { cell.RowId, cell.ColumnId });
            entity.Property(cell => cell.Value).HasMaxLength(4000).IsRequired();
            entity.HasOne(cell => cell.Row).WithMany().HasForeignKey(cell => cell.RowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cell => cell.Column).WithMany().HasForeignKey(cell => cell.ColumnId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetaIntegerCellEntity>(entity =>
        {
            entity.ToTable("meta_integer_cells");
            entity.HasKey(cell => new { cell.RowId, cell.ColumnId });
            entity.HasOne(cell => cell.Row).WithMany().HasForeignKey(cell => cell.RowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cell => cell.Column).WithMany().HasForeignKey(cell => cell.ColumnId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetaAssociationCellEntity>(entity =>
        {
            entity.ToTable("meta_association_cells");
            entity.HasKey(cell => new { cell.RowId, cell.ColumnId });
            entity.HasOne(cell => cell.Row).WithMany().HasForeignKey(cell => cell.RowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cell => cell.Column).WithMany().HasForeignKey(cell => cell.ColumnId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cell => cell.TargetRow).WithMany().HasForeignKey(cell => cell.TargetRowId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
