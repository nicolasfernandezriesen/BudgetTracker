using System;
using System.Collections.Generic;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace BudgetTracker.Data;

public partial class BudgettrackerdbContext : DbContext
{
    public BudgettrackerdbContext()
    {
    }

    public BudgettrackerdbContext(DbContextOptions<BudgettrackerdbContext> options) : base(options) { }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<Category> Categorys { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<MonthlyTotal> MonthlyTotals { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=budgettrackerdb;user id=root;password=jameme29", ServerVersion.Parse("8.0.37-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.BillsId).HasName("PRIMARY");

            entity.ToTable("bills");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.BillsId).HasColumnName("bills_id");
            entity.Property(e => e.BillsAmount)
                .HasPrecision(10)
                .HasColumnName("bills_amount");
            entity.Property(e => e.BillsDate).HasColumnName("bills_date");
            entity.Property(e => e.BillsDesc)
                .HasMaxLength(250)
                .HasColumnName("bills_desc");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Bills)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bills_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Bills)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bills_ibfk_2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("categorys");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(250)
                .HasColumnName("category_name");
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasKey(e => e.IncomeId).HasName("PRIMARY");

            entity.ToTable("incomes");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.IncomeId).HasColumnName("income_id");
            entity.Property(e => e.IncomeAmount)
                .HasPrecision(10)
                .HasColumnName("income_amount");
            entity.Property(e => e.IncomeDate).HasColumnName("income_date");
            entity.Property(e => e.IncomeDesc)
                .HasMaxLength(250)
                .HasColumnName("income_desc");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("incomes_ibfk_1");
        });

        modelBuilder.Entity<MonthlyTotal>(entity =>
        {
            entity.HasKey(e => e.MonthlyTotalsId).HasName("PRIMARY");

            entity.ToTable("monthly_totals");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.MonthlyTotalsId).HasColumnName("monthly_totals_id");
            entity.Property(e => e.MonthlyTotalsMonth).HasColumnName("monthly_totals_month");
            entity.Property(e => e.MonthlyTotalsYear).HasColumnName("monthly_totals_year");
            entity.Property(e => e.TotalBill)
                .HasPrecision(10)
                .HasColumnName("total_bill");
            entity.Property(e => e.TotalIncome)
                .HasPrecision(10)
                .HasColumnName("total_income");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.MonthlyTotals)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("monthly_totals_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(250)
                .HasColumnName("user_email");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(250)
                .HasColumnName("user_password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
