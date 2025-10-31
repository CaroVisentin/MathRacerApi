using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Configuration
{
        public class MathiRacerDbContext : DbContext
        {
            // Constructor para migraciones CLI (diseño)
            public MathiRacerDbContext() : base(GetOptionsFromEnv()) {}

            private static DbContextOptions<MathiRacerDbContext> GetOptionsFromEnv()
            {
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Intentar cargar el .env si no está definida la variable
                    try
                    {
                        DotNetEnv.Env.Load(); // Carga .env por defecto
                        connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
                    }
                    catch { /* Ignorar si no existe DotNetEnv o el archivo */ }
                }
                var optionsBuilder = new DbContextOptionsBuilder<MathiRacerDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                return optionsBuilder.Options;
            }
        public MathiRacerDbContext(DbContextOptions<MathiRacerDbContext> options)
            : base(options)
            {
            }

        // DbSets
        public DbSet<CoinPackageEntity> CoinPackages { get; set; } = null!;
        public DbSet<DifficultyEntity> Difficulties { get; set; } = null!;
        public DbSet<EnergyConfigurationEntity> EnergyConfigurations { get; set; } = null!;
        public DbSet<EnergyEntity> Energies { get; set; } = null!;
        public DbSet<FriendshipEntity> Friendships { get; set; } = null!;
        public DbSet<LevelEntity> Levels { get; set; } = null!;
        public DbSet<OperationEntity> Operations { get; set; } = null!;
        public DbSet<PaymentMethodEntity> PaymentMethods { get; set; } = null!;
        public DbSet<PlayerEntity> Players { get; set; } = null!;
        public DbSet<PlayerProductEntity> PlayerProducts { get; set; } = null!;
        public DbSet<PlayerWildcardEntity> PlayerWildcards { get; set; } = null!;
        public DbSet<ProductEntity> Products { get; set; } = null!;
        public DbSet<ProductTypeEntity> ProductTypes { get; set; } = null!;
        public DbSet<PurchaseEntity> Purchases { get; set; } = null!;
        public DbSet<RarityEntity> Rarities { get; set; } = null!;
        public DbSet<RequestStatusEntity> RequestStatuses { get; set; } = null!;
        public DbSet<ResultTypeEntity> ResultTypes { get; set; } = null!;
        public DbSet<WildcardEntity> Wildcards { get; set; } = null!;
        public DbSet<WorldEntity> Worlds { get; set; } = null!;
        public DbSet<WorldOperationEntity> WorldOperations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- COIN PACKAGE ---
            modelBuilder.Entity<CoinPackageEntity>(entity =>
            {
                entity.ToTable("CoinPackage");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CoinAmount)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasMany(e => e.Purchases)
                    .WithOne(p => p.CoinPackage)
                    .HasForeignKey(p => p.CoinPackageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- DIFFICULTY ---
            modelBuilder.Entity<DifficultyEntity>(entity =>
            {
                entity.ToTable("Difficulty");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasMany(e => e.Worlds)
                    .WithOne(w => w.Difficulty)
                    .HasForeignKey(w => w.DifficultyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- ENERGY CONFIGURATION ---
            modelBuilder.Entity<EnergyConfigurationEntity>(entity =>
            {
                entity.ToTable("EnergyConfiguration");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxAmount)
                    .IsRequired();
            });

            // --- ENERGY ---
            modelBuilder.Entity<EnergyEntity>(entity =>
            {
                entity.ToTable("Energy");
                entity.HasKey(e => e.PlayerId);
                entity.Property(e => e.Amount)
                    .IsRequired()
                    .HasDefaultValue(3);
                entity.Property(e => e.LastConsumptionDate)
                    .IsRequired()
                    .HasColumnType("datetime")
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

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Player1)
                    .WithMany(p => p.Friendships1)
                    .HasForeignKey(e => e.PlayerId1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Player2)
                    .WithMany(p => p.Friendships2)
                    .HasForeignKey(e => e.PlayerId2)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestStatus)
                    .WithMany(rs => rs.Friendships)
                    .HasForeignKey(e => e.RequestStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- LEVEL ---
            modelBuilder.Entity<LevelEntity>(entity =>
            {
                entity.ToTable("Level");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Number).IsRequired();

                entity.Property(e => e.TermsCount).IsRequired();
                entity.Property(e => e.VariablesCount).IsRequired();
          
                entity.HasOne(e => e.ResultType)
                    .WithMany(r => r.Levels)
                    .HasForeignKey(e => e.ResultTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.World)
                    .WithMany(w => w.Levels)
                    .HasForeignKey(e => e.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- OPERATION ---
            modelBuilder.Entity<OperationEntity>(entity =>
            {
                entity.ToTable("Operation");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Sign)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.HasMany(e => e.WorldOperations)
                    .WithOne(wo => wo.Operation)
                    .HasForeignKey(wo => wo.OperationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PAYMENT METHOD ---
            modelBuilder.Entity<PaymentMethodEntity>(entity =>
            {
                entity.ToTable("PaymentMethod");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // --- PLAYER ---
            modelBuilder.Entity<PlayerEntity>(entity =>
            {
                entity.ToTable("Player");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Uid)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Coins)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Points)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Deleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasOne(e => e.LastLevel)
                    .WithMany(l => l.Players)
                    .HasForeignKey(e => e.LastLevelId)
                    .OnDelete(DeleteBehavior.Restrict);
   

            });

            // --- PLAYER PRODUCT ---
            modelBuilder.Entity<PlayerProductEntity>(entity =>
            {
                entity.ToTable("PlayerProduct");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasOne(e => e.Player)
                    .WithMany(p => p.PlayerProducts)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.PlayerProducts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // --- PLAYER WILDCARD ---
            modelBuilder.Entity<PlayerWildcardEntity>(entity =>
            {
                entity.ToTable("PlayerWildcard");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Quantity)
                    .IsRequired();
                entity.HasOne(e => e.Player)
                    .WithMany()
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- PRODUCT ---
            modelBuilder.Entity<ProductEntity>(entity =>
            {
                entity.ToTable("Product");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ProductType)
                    .WithMany(pt => pt.Products)
                    .HasForeignKey(e => e.ProductTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.PlayerProducts)
                    .WithOne(pp => pp.Product)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Rarity)
                    .WithMany(r => r.Products)
                    .HasForeignKey(r => r.RarityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PRODUCT TYPE ---
            modelBuilder.Entity<ProductTypeEntity>(entity =>
            {
                entity.ToTable("ProductType");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasMany(e => e.Products)
                    .WithOne(p => p.ProductType)
                    .HasForeignKey(p => p.ProductTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PURCHASE ---
            modelBuilder.Entity<PurchaseEntity>(entity =>
            {
                entity.ToTable("Purchase");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Date)
                    .IsRequired()
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
            });

            // --- RARITY ---
            modelBuilder.Entity<RarityEntity>(entity =>
            {
                entity.ToTable("Rarity");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Rarity)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasMany(e => e.Products)
                  .WithOne(r => r.Rarity)
                  .HasForeignKey(r => r.RarityId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            // --- REQUEST STATUS ---
            modelBuilder.Entity<RequestStatusEntity>(entity =>
            {
                entity.ToTable("RequestStatus");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.HasMany(e => e.Friendships)
                    .WithOne(f => f.RequestStatus)
                    .HasForeignKey(f => f.RequestStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- RESULT TYPE ---
            modelBuilder.Entity<ResultTypeEntity>(entity =>
            {
                entity.ToTable("ResultType");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.HasMany(e => e.Levels)
                    .WithOne(l => l.ResultType)
                    .HasForeignKey(l => l.ResultTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- WILDCARD ---
            modelBuilder.Entity<WildcardEntity>(entity =>
            {
                entity.ToTable("Wildcard");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(255);
            });

            // --- WORLD ---
            modelBuilder.Entity<WorldEntity>(entity =>
            {
                entity.ToTable("World");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OptionsCount).IsRequired();
                entity.Property(e => e.TimePerEquation).IsRequired();
                entity.Property(e => e.OptionRangeMin).IsRequired();
                entity.Property(e => e.OptionRangeMax).IsRequired();
                entity.Property(e => e.NumberRangeMin).IsRequired();
                entity.Property(e => e.NumberRangeMax).IsRequired();
          
                entity.HasOne(e => e.Difficulty)
                    .WithMany(d => d.Worlds)
                    .HasForeignKey(e => e.DifficultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Levels)
                    .WithOne(l => l.World)
                    .HasForeignKey(l => l.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.WorldOperations)
                    .WithOne(wo => wo.World)
                    .HasForeignKey(wo => wo.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);
   

            });

            // --- WORLD OPERATION ---
            modelBuilder.Entity<WorldOperationEntity>(entity =>
            {
                entity.ToTable("WorldOperation");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(e => e.World)
                    .WithMany(w => w.WorldOperations)
                    .HasForeignKey(e => e.WorldId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Operation)
                    .WithMany(o => o.WorldOperations)
                    .HasForeignKey(e => e.OperationId)
                    .OnDelete(DeleteBehavior.Restrict);
   

            });
        }
    }
}
