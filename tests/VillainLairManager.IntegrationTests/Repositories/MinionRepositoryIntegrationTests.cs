using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;
using FluentAssertions;

namespace VillainLairManager.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for MinionRepository
/// Tests actual database operations with a test database
/// </summary>
public class MinionRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private readonly MinionRepository _repository;

    public MinionRepositoryIntegrationTests()
    {
        _repository = new MinionRepository(Context);
    }

    [Fact]
    public void Insert_And_GetById_ReturnsInsertedMinion()
    {
        // Arrange
        var minion = new Minion
        {
            Name = "Test Minion",
            SkillLevel = 5,
            Specialty = "Hacking",
            LoyaltyScore = 75,
            SalaryDemand = 5000m,
            MoodStatus = "Happy",
            LastMoodUpdate = DateTime.Now
        };

        // Act
        _repository.Insert(minion);
        var retrieved = _repository.GetById(minion.MinionId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Minion");
        retrieved.SkillLevel.Should().Be(5);
        retrieved.Specialty.Should().Be("Hacking");
        retrieved.LoyaltyScore.Should().Be(75);
        retrieved.SalaryDemand.Should().Be(5000m);
    }

    [Fact]
    public void GetAll_ReturnsAllMinions()
    {
        // Arrange
        var minion1 = new Minion { Name = "Minion 1", SkillLevel = 3, Specialty = "Combat", LoyaltyScore = 50, SalaryDemand = 3000m, MoodStatus = "Content", LastMoodUpdate = DateTime.Now };
        var minion2 = new Minion { Name = "Minion 2", SkillLevel = 7, Specialty = "Hacking", LoyaltyScore = 80, SalaryDemand = 7000m, MoodStatus = "Content", LastMoodUpdate = DateTime.Now };
        var minion3 = new Minion { Name = "Minion 3", SkillLevel = 5, Specialty = "Explosives", LoyaltyScore = 60, SalaryDemand = 4500m, MoodStatus = "Content", LastMoodUpdate = DateTime.Now };

        _repository.Insert(minion1);
        _repository.Insert(minion2);
        _repository.Insert(minion3);

        // Act
        var allMinions = _repository.GetAll().ToList();

        // Assert
        allMinions.Should().HaveCount(3);
        allMinions.Should().Contain(m => m.Name == "Minion 1");
        allMinions.Should().Contain(m => m.Name == "Minion 2");
        allMinions.Should().Contain(m => m.Name == "Minion 3");
    }

    [Fact]
    public void Update_ModifiesExistingMinion()
    {
        // Arrange
        var minion = new Minion
        {
            Name = "Original Name",
            SkillLevel = 3,
            Specialty = "Combat",
            LoyaltyScore = 50,
            SalaryDemand = 3000m,
            MoodStatus = "Content",
            LastMoodUpdate = DateTime.Now
        };
        _repository.Insert(minion);

        // Act
        minion.Name = "Updated Name";
        minion.LoyaltyScore = 75;
        minion.SalaryDemand = 5000m;
        _repository.Update(minion);

        var updated = _repository.GetById(minion.MinionId);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.LoyaltyScore.Should().Be(75);
        updated.SalaryDemand.Should().Be(5000m);
    }

    [Fact]
    public void Delete_RemovesMinion()
    {
        // Arrange
        var minion = new Minion
        {
            Name = "To Be Deleted",
            SkillLevel = 4,
            Specialty = "Engineering",
            LoyaltyScore = 60,
            SalaryDemand = 4000m,
            MoodStatus = "Content",
            LastMoodUpdate = DateTime.Now
        };
        _repository.Insert(minion);
        var minionId = minion.MinionId;

        // Act
        _repository.Delete(minionId);
        var deleted = _repository.GetById(minionId);

        // Assert
        deleted.Should().BeNull();
    }

    [Fact]
    public void GetById_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = _repository.GetById(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Insert_MultipleMinionsSameName_AllInserted()
    {
        // Arrange - Testing that multiple minions can have the same name
        var minion1 = new Minion { Name = "Bob", SkillLevel = 3, Specialty = "Combat", LoyaltyScore = 50, SalaryDemand = 3000m, MoodStatus = "Content", LastMoodUpdate = DateTime.Now };
        var minion2 = new Minion { Name = "Bob", SkillLevel = 5, Specialty = "Hacking", LoyaltyScore = 60, SalaryDemand = 5000m, MoodStatus = "Content", LastMoodUpdate = DateTime.Now };

        // Act
        _repository.Insert(minion1);
        _repository.Insert(minion2);
        var allBobs = _repository.GetAll().Where(m => m.Name == "Bob").ToList();

        // Assert
        allBobs.Should().HaveCount(2);
        allBobs[0].MinionId.Should().NotBe(allBobs[1].MinionId);
    }

    [Fact]
    public void Update_LoyaltyAndMood_PersistsChanges()
    {
        // Arrange
        var minion = new Minion
        {
            Name = "Loyalty Test",
            SkillLevel = 5,
            Specialty = "Hacking",
            LoyaltyScore = 50,
            SalaryDemand = 5000m,
            MoodStatus = "Grumpy",
            LastMoodUpdate = DateTime.Now.AddDays(-10)
        };
        _repository.Insert(minion);

        // Act - Simulate loyalty update
        minion.LoyaltyScore = 85;
        minion.MoodStatus = "Happy";
        minion.LastMoodUpdate = DateTime.Now;
        _repository.Update(minion);

        var updated = _repository.GetById(minion.MinionId);

        // Assert
        updated.Should().NotBeNull();
        updated!.LoyaltyScore.Should().Be(85);
        updated.MoodStatus.Should().Be("Happy");
        updated.LastMoodUpdate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Insert_WithSchemeAssignment_PersistsRelationship()
    {
        // Arrange
        var minion = new Minion
        {
            Name = "Scheme Worker",
            SkillLevel = 6,
            Specialty = "Engineering",
            LoyaltyScore = 70,
            SalaryDemand = 6000m,
            CurrentSchemeId = 1,
            CurrentBaseId = 2,
            MoodStatus = "Content",
            LastMoodUpdate = DateTime.Now
        };

        // Act
        _repository.Insert(minion);
        var retrieved = _repository.GetById(minion.MinionId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.CurrentSchemeId.Should().Be(1);
        retrieved.CurrentBaseId.Should().Be(2);
    }
}