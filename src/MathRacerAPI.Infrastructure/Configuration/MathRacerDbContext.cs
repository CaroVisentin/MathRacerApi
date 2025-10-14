using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Configuration
{
    public class MathRacerDbContext : DbContext
    {
        public MathRacerDbContext(DbContextOptions<MathRacerDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<PlayerEntity> Players { get; set; } = null!;
        public DbSet<InventoryEntity> Inventories { get; set; } = null!;
        public DbSet<EnergyEntity> Energies { get; set; } = null!;
        public DbSet<FriendshipEntity> Friendships { get; set; } = null!;
        public DbSet<LevelEntity> Levels { get; set; } = null!;
        public DbSet<DifficultyEntity> Difficulties { get; set; } = null!;
        public DbSet<OperationEntity> Operations { get; set; } = null!;
        public DbSet<PlayerProductEntity> PlayerProducts { get; set; } = null!;
        public DbSet<ProductEntity> Products { get; set; } = null!;
        public DbSet<RequestStatusEntity> RequestStatuses { get; set; } = null!;
        public DbSet<ResultTypeEntity> ResultTypes { get; set; } = null!;
        public DbSet<SubproductEntity> Subproducts { get; set; } = null!;
        public DbSet<WorldEntity> Worlds { get; set; } = null!;
        public DbSet<WorldOperationEntity> WorldOperations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- PLAYER ---
            modelBuilder.Entity<PlayerEntity>(entity =>
            {
                entity.ToTable("Player");

                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Email)
                        .IsRequired()
                        .HasMaxLength(100);

                entity.HasIndex(p => p.Email).IsUnique();

                entity.Property(p => p.Password)
                        .IsRequired()
                        .HasMaxLength(255);

                entity.Property(p => p.Coins)
                        .IsRequired()
                        .HasColumnType("decimal(18,2)")
                        .HasDefaultValue(0);

                entity.Property(p => p.Points)
                        .IsRequired()
                        .HasDefaultValue(0);

                entity.Property(p => p.Deleted)
                        .IsRequired()
                        .HasDefaultValue(false);

                entity.HasOne(p => p.LastLevel)
                    .WithMany(l => l.Players)
                    .HasForeignKey(p => p.LastLevelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Inventory)
                    .WithOne(i => i.Player)
                    .HasForeignKey<InventoryEntity>(i => i.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Energy)
                    .WithOne(e => e.Player)
                    .HasForeignKey<EnergyEntity>(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
         
            });

            // --- INVENTORY ---
            modelBuilder.Entity<InventoryEntity>(entity =>
            {
                entity.ToTable("Inventory");

                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).ValueGeneratedOnAdd();

                entity.Property(i => i.Wildcard1Count)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.Property(i => i.Wildcard2Count)
                        .IsRequired()
                        .HasDefaultValue(0);

                entity.Property(i => i.Wildcard3Count)
                        .IsRequired()
                        .HasDefaultValue(0);

                entity.HasOne(i => i.Player)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<InventoryEntity>(i => i.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
             });

            // --- ENERGY ---
            modelBuilder.Entity<EnergyEntity>(entity =>
            {
                entity.ToTable("Energy");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Amount)
                      .IsRequired()
                      .HasDefaultValue(3);

                entity.Property(e => e.LastConsumptionDate)
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()"); 

                entity.HasOne(e => e.Player)
                    .WithOne(p => p.Energy)
                    .HasForeignKey<EnergyEntity>(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- FRIENDSHIP ---
            modelBuilder.Entity<FriendshipEntity>(entity =>
            {
                entity.ToTable("Friendship");

                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).ValueGeneratedOnAdd();

                entity.HasOne(f => f.Player1)
                    .WithMany(p => p.Friendships1)
                    .HasForeignKey(f => f.PlayerId1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Player2)
                    .WithMany(p => p.Friendships2)
                    .HasForeignKey(f => f.PlayerId2)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.RequestStatus)
                    .WithMany(rs => rs.Friendships)
                    .HasForeignKey(f => f.RequestStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- LEVEL ---
            modelBuilder.Entity<LevelEntity>(entity =>
            {
                entity.ToTable("Level");

                entity.HasKey(l => l.Id);
                entity.Property(l => l.Id).ValueGeneratedOnAdd();

                entity.Property(l => l.Number).IsRequired();

                entity.Property(l => l.TermsCount).IsRequired();
                entity.Property(l => l.VariablesCount).IsRequired();
          
                entity.HasOne(l => l.ResultType)
                    .WithMany(r => r.Levels)
                    .HasForeignKey(l => l.ResultTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.World)
                    .WithMany(w => w.Levels)
                    .HasForeignKey(l => l.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);       
            });

            // --- DIFFICULTY ---
            modelBuilder.Entity<DifficultyEntity>(entity =>
            {
                entity.ToTable("Difficulty");

                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).ValueGeneratedOnAdd();

                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(50);
             
            });

            // --- OPERATION ---
            modelBuilder.Entity<OperationEntity>(entity =>
            {
                entity.ToTable("Operation");

                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).ValueGeneratedOnAdd();

                entity.Property(o => o.Sign)
                      .IsRequired()
                      .HasMaxLength(5);

                entity.Property(o => o.Description)
                        .HasMaxLength(255);

                entity.HasMany(o => o.WorldOperations)
                    .WithOne(wo => wo.Operation)
                    .HasForeignKey(wo => wo.OperationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PLAYERPRODUCT ---
            modelBuilder.Entity<PlayerProductEntity>(entity =>
            {
                entity.ToTable("PlayerProduct");

                entity.HasKey(pp => pp.Id);
                entity.Property(pp => pp.Id).ValueGeneratedOnAdd();

                entity.Property(pp => pp.Quantity)
                      .IsRequired()
                      .HasDefaultValue(1);

                entity.Property(pp => pp.IsActive)
                        .IsRequired()
                        .HasDefaultValue(false);

                entity.HasOne(pp => pp.Player)
                    .WithMany(p => p.PlayerProducts)
                    .HasForeignKey(pp => pp.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Product)
                    .WithMany(p => p.PlayerProducts)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // --- PRODUCT ---
            modelBuilder.Entity<ProductEntity>(entity =>
            {
                entity.ToTable("Product");

                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Description)
                      .HasMaxLength(255);

                entity.Property(p => p.Price)
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Subproduct)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SubproductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.PlayerProducts)
                    .WithOne(pp => pp.Product)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- REQUEST STATUS ---
            modelBuilder.Entity<RequestStatusEntity>(entity =>
            {
                entity.ToTable("RequestStatus");

                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Id).ValueGeneratedOnAdd();

                entity.Property(rs => rs.Name)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(rs => rs.Description)
                        .HasMaxLength(255);

                entity.HasMany(rs => rs.Friendships)
                    .WithOne(f => f.RequestStatus)
                    .HasForeignKey(f => f.RequestStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // --- RESULT TYPE ---
            modelBuilder.Entity<ResultTypeEntity>(entity =>
            {
                entity.ToTable("ResultType");

                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedOnAdd();

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(r => r.Description)
                        .HasMaxLength(255);

                entity.HasMany(r => r.Levels)
                    .WithOne(l => l.ResultType)
                    .HasForeignKey(l => l.ResultTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // --- SUBPRODUCT ---
            modelBuilder.Entity<SubproductEntity>(entity =>
            {
                entity.ToTable("Subproduct");

                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedOnAdd();

                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(100);
             
                entity.HasMany(s => s.Products)
                    .WithOne(p => p.Subproduct)
                    .HasForeignKey(p => p.SubproductId)
                    .OnDelete(DeleteBehavior.Restrict);
    
            });

            // --- WORLD ---
            modelBuilder.Entity<WorldEntity>(entity =>
            {
                entity.ToTable("World");

                entity.HasKey(w => w.Id);
                entity.Property(w => w.Id).ValueGeneratedOnAdd();

                entity.Property(w => w.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(w => w.OptionsCount).IsRequired();
                entity.Property(w => w.TimePerEquation).IsRequired();
                entity.Property(w => w.OptionRangeMin).IsRequired();
                entity.Property(w => w.OptionRangeMax).IsRequired();
                entity.Property(w => w.NumberRangeMin).IsRequired();
                entity.Property(w => w.NumberRangeMax).IsRequired();
          
                entity.HasOne(w => w.Difficulty)
                    .WithMany(d => d.Worlds)
                    .HasForeignKey(w => w.DifficultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(w => w.Levels)
                    .WithOne(l => l.World)
                    .HasForeignKey(l => l.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(w => w.WorldOperations)
                    .WithOne(wo => wo.World)
                    .HasForeignKey(wo => wo.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);
               
            });


            // --- WORLDOPERATION ---
            modelBuilder.Entity<WorldOperationEntity>(entity =>
            {
                entity.ToTable("WorldOperation");

                entity.HasKey(wo => wo.Id);
                entity.Property(wo => wo.Id).ValueGeneratedOnAdd();

                entity.HasOne(wo => wo.World)
                    .WithMany(w => w.WorldOperations)
                    .HasForeignKey(wo => wo.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wo => wo.Operation)
                    .WithMany(o => o.WorldOperations)
                    .HasForeignKey(wo => wo.OperationId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

        }
    }
}
