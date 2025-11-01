using MathRacerAPI.Domain.Models;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using MathRacerAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MathRacerAPI.Tests.Repositories;

public class RankingRepositoryTests
{
    private MathiRacerDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MathiRacerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MathiRacerDbContext(options);
    }

    private void SeedTestData(MathiRacerDbContext context)
    {
        var players = new List<PlayerEntity>
        {
            new() { Id = 1, Name = "Player1", Points = 100, Email = "p1@test.com", Uid = "uid1", Deleted = false },
            new() { Id = 2, Name = "Player2", Points = 200, Email = "p2@test.com", Uid = "uid2", Deleted = false },
            new() { Id = 3, Name = "Player3", Points = 150, Email = "p3@test.com", Uid = "uid3", Deleted = false },
            new() { Id = 4, Name = "Player4", Points = 300, Email = "p4@test.com", Uid = "uid4", Deleted = false },
            new() { Id = 5, Name = "Player5", Points = 50, Email = "p5@test.com", Uid = "uid5", Deleted = false },
            new() { Id = 6, Name = "Player6", Points = 250, Email = "p6@test.com", Uid = "uid6", Deleted = false },
            new() { Id = 7, Name = "Player7", Points = 80, Email = "p7@test.com", Uid = "uid7", Deleted = false },
            new() { Id = 8, Name = "Player8", Points = 350, Email = "p8@test.com", Uid = "uid8", Deleted = false },
            new() { Id = 9, Name = "Player9", Points = 180, Email = "p9@test.com", Uid = "uid9", Deleted = false },
            new() { Id = 10, Name = "Player10", Points = 120, Email = "p10@test.com", Uid = "uid10", Deleted = false },
            new() { Id = 11, Name = "Player11", Points = 90, Email = "p11@test.com", Uid = "uid11", Deleted = false },
            new() { Id = 12, Name = "Player12", Points = 280, Email = "p12@test.com", Uid = "uid12", Deleted = false }
        };
        
        context.Players.AddRange(players);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldReturnTop10PlayersOrderedByPoints()
    {
        // Arrange
        using var context = GetInMemoryContext();
        SeedTestData(context);
        var repository = new RankingRepository(context);
        
        // Act
        var (top10, position) = await repository.GetTop10WithPlayerPositionAsync(1);
        
        // Assert
        Assert.Equal(10, top10.Count);
        Assert.Equal(350, top10[0].Points); // Player8
        Assert.Equal(300, top10[1].Points); // Player4
        Assert.Equal(280, top10[2].Points); // Player12
        Assert.Equal(250, top10[3].Points); // Player6
        Assert.Equal(200, top10[4].Points); // Player2
        Assert.Equal(180, top10[5].Points); // Player9
        Assert.Equal(150, top10[6].Points); // Player3
        Assert.Equal(120, top10[7].Points); // Player10
        Assert.Equal(100, top10[8].Points); // Player1
        Assert.Equal(90, top10[9].Points);  // Player11
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldReturnCorrectPlayerPosition()
    {
        // Arrange
        using var context = GetInMemoryContext();
        SeedTestData(context);
        var repository = new RankingRepository(context);
        
        // Act
        var (_, position) = await repository.GetTop10WithPlayerPositionAsync(1); // Player1 with 100 points
        
        // Assert
        Assert.Equal(9, position); // Player1 should be in 9th position (100 points)
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldReturnZeroForNonExistentPlayer()
    {
        // Arrange
        using var context = GetInMemoryContext();
        SeedTestData(context);
        var repository = new RankingRepository(context);
        
        // Act
        var (top10, position) = await repository.GetTop10WithPlayerPositionAsync(999);
        
        // Assert
        Assert.Equal(10, top10.Count);
        Assert.Equal(0, position);
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldExcludeDeletedPlayers()
    {
        // Arrange
        using var context = GetInMemoryContext();
        SeedTestData(context);
        
        // Mark one player as deleted
        var playerToDelete = context.Players.First(p => p.Id == 8); // Player8 with highest points
        playerToDelete.Deleted = true;
        context.SaveChanges();
        
        var repository = new RankingRepository(context);
        
        // Act
        var (top10, position) = await repository.GetTop10WithPlayerPositionAsync(4); // Player4
        
        // Assert
        Assert.Equal(10, top10.Count);
        Assert.Equal(300, top10[0].Points); // Player4 should now be first (Player8 deleted)
        Assert.Equal(1, position); // Player4 should be in 1st position
        Assert.DoesNotContain(top10, p => p.Name == "Player8"); // Deleted player should not appear
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldReturnLessThan10WhenLessPlayersExist()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var players = new List<PlayerEntity>
        {
            new() { Id = 1, Name = "Player1", Points = 100, Email = "p1@test.com", Uid = "uid1", Deleted = false },
            new() { Id = 2, Name = "Player2", Points = 200, Email = "p2@test.com", Uid = "uid2", Deleted = false },
            new() { Id = 3, Name = "Player3", Points = 150, Email = "p3@test.com", Uid = "uid3", Deleted = false }
        };
        context.Players.AddRange(players);
        context.SaveChanges();
        
        var repository = new RankingRepository(context);
        
        // Act
        var (top10, position) = await repository.GetTop10WithPlayerPositionAsync(2);
        
        // Assert
        Assert.Equal(3, top10.Count);
        Assert.Equal(200, top10[0].Points); // Player2
        Assert.Equal(150, top10[1].Points); // Player3
        Assert.Equal(100, top10[2].Points); // Player1
        Assert.Equal(1, position); // Player2 should be in 1st position
    }

    [Fact]
    public async Task GetTop10WithPlayerPositionAsync_ShouldHandleEmptyDatabase()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = new RankingRepository(context);
        
        // Act
        var (top10, position) = await repository.GetTop10WithPlayerPositionAsync(1);
        
        // Assert
        Assert.Empty(top10);
        Assert.Equal(0, position);
    }
}