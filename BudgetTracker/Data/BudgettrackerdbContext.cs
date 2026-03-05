using System;
using System.Collections.Generic;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data;

public partial class BudgettrackerdbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    private readonly IConfiguration _configuration;
    public BudgettrackerdbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<Category> Categorys { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<MonthlyTotal> MonthlyTotals { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci");

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
            entity.HasIndex(e => e.CategoryId, "incomes_ibfk_2_idx");

            entity.Property(e => e.IncomeId).HasColumnName("income_id");
            entity.Property(e => e.IncomeAmount)
                .HasPrecision(10)
                .HasColumnName("income_amount");
            entity.Property(e => e.IncomeDate).HasColumnName("income_date");
            entity.Property(e => e.IncomeDesc)
                .HasMaxLength(250)
                .HasColumnName("income_desc");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("incomes_ibfk_1");

            entity.HasOne(d => d.Category).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("incomes_ibfk_2");
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

        // Configure User table mapping
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.UserName).HasColumnName("user_name");
            entity.Property(e => e.NormalizedUserName).HasColumnName("normalized_user_name");
            entity.Property(e => e.Email).HasColumnName("user_email");
            entity.Property(e => e.NormalizedEmail).HasColumnName("normalized_email");
            entity.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");
            entity.Property(e => e.PasswordHash).HasColumnName("user_password");
            entity.Property(e => e.SecurityStamp).HasColumnName("security_stamp");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
            entity.Property(e => e.LockoutEnabled).HasColumnName("lockout_enabled");
            entity.Property(e => e.AccessFailedCount).HasColumnName("access_failed_count");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("user_email");
            entity.HasIndex(e => e.UserName).IsUnique().HasDatabaseName("user_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
