using FluentAssertions;
using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.UnitTests.Services;

/// <summary>
/// Unit tests for MinionService business logic
/// Tests BR-M-001 (Loyalty Decay and Growth), BR-M-002 (Mood Determination), and BR-M-003 (Skill Matching)
/// </summary>
public class MinionServiceTests
{
    private readonly Mock<IMinionRepository> _mockMinionRepository;
    private readonly MinionService _sut; // System Under Test

    public MinionServiceTests()
    {
        _mockMinionRepository = new Mock<IMinionRepository>();
        _sut = new MinionService(_mockMinionRepository.Object);
    }

    #region BR-M-001: Loyalty Decay and Growth Tests

    [Fact]
    public void UpdateMinionLoyalty_WhenSatisfied_IncreasesLoyaltyBy3()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 70,
            SalaryDemand = 5000m
        };
        decimal actualSalaryPaid = 5000m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(73);
        _mockMinionRepository.Verify(r => r.Update(minion), Times.AtLeastOnce());
    }

    [Fact]
    public void UpdateMinionLoyalty_WhenOverpaid_IncreasesLoyaltyBy3()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 70,
            SalaryDemand = 5000m
        };
        decimal actualSalaryPaid = 6000m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(73);
    }

    [Fact]
    public void UpdateMinionLoyalty_WhenUnderpaid_DecreasesLoyaltyBy5()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 70,
            SalaryDemand = 5000m
        };
        decimal actualSalaryPaid = 4000m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(65);
    }

    [Fact]
    public void UpdateMinionLoyalty_AtMinimumBoundary_ClampsToZero()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 3,
            SalaryDemand = 3000m
        };
        decimal actualSalaryPaid = 2000m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(0);
    }

    [Fact]
    public void UpdateMinionLoyalty_AtMaximumBoundary_ClampsTo100()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 98,
            SalaryDemand = 3000m
        };
        decimal actualSalaryPaid = 4000m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(100);
    }

    [Fact]
    public void UpdateMinionLoyalty_WithZeroSalaryDemand_IncreasesLoyalty()
    {
        // Arrange - Edge case: If minion has no salary demand (0), treat as satisfied
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 50,
            SalaryDemand = 0m
        };
        decimal actualSalaryPaid = 0m;

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionLoyalty(minion, actualSalaryPaid);

        // Assert
        minion.LoyaltyScore.Should().Be(53);
    }

    #endregion

    #region BR-M-002: Mood Determination Tests

    [Fact]
    public void UpdateMinionMood_WithHighLoyalty_SetsMoodToHappy()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 85
        };

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionMood(minion);

        // Assert
        minion.MoodStatus.Should().Be(AppSettings.Instance.MoodHappy);
        minion.LastMoodUpdate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        _mockMinionRepository.Verify(r => r.Update(minion), Times.Once());
    }

    [Fact]
    public void UpdateMinionMood_WithMediumLoyalty_SetsMoodToGrumpy()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 55
        };

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionMood(minion);

        // Assert
        minion.MoodStatus.Should().Be(AppSettings.Instance.MoodGrumpy);
    }

    [Fact]
    public void UpdateMinionMood_WithLowLoyalty_SetsMoodToPlottingBetrayal()
    {
        // Arrange
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 25
        };

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionMood(minion);

        // Assert
        minion.MoodStatus.Should().Be(AppSettings.Instance.MoodBetrayal);
    }

    [Fact]
    public void UpdateMinionMood_AtHighThreshold_SetsMoodCorrectly()
    {
        // Arrange - Exactly at threshold (70)
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 70
        };

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionMood(minion);

        // Assert - At threshold is NOT > 70, so should be Grumpy
        minion.MoodStatus.Should().Be(AppSettings.Instance.MoodGrumpy);
    }

    [Fact]
    public void UpdateMinionMood_AtLowThreshold_SetsMoodCorrectly()
    {
        // Arrange - Exactly at threshold (40)
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = 40
        };

        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinionMood(minion);

        // Assert - At threshold is NOT < 40, so should be Grumpy
        minion.MoodStatus.Should().Be(AppSettings.Instance.MoodGrumpy);
    }

    #endregion

    #region BR-M-003: Skill Matching and Validation Tests

    [Theory]
    [InlineData("Hacking", true)]
    [InlineData("Explosives", true)]
    [InlineData("Disguise", true)]
    [InlineData("Combat", true)]
    [InlineData("Engineering", true)]
    [InlineData("Piloting", true)]
    [InlineData("InvalidSpecialty", false)]
    [InlineData("", false)]
    public void IsValidSpecialty_ValidatesSpecialtyCorrectly(string? specialty, bool expectedResult)
    {
        // Act
        var result = _sut.IsValidSpecialty(specialty!);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsValidSpecialty_WithNull_ReturnsFalse()
    {
        // Act
        var result = _sut.IsValidSpecialty(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CRUD Operation Tests

    [Fact]
    public void GetAllMinions_ReturnsAllMinionsFromRepository()
    {
        // Arrange
        var expectedMinions = new List<Minion>
            {
                new Minion { MinionId = 1, Name = "Minion 1" },
                new Minion { MinionId = 2, Name = "Minion 2" }
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(expectedMinions);

        // Act
        var result = _sut.GetAllMinions();

        // Assert
        result.Should().BeEquivalentTo(expectedMinions);
        _mockMinionRepository.Verify(r => r.GetAll(), Times.Once());
    }

    [Fact]
    public void GetMinionById_ReturnsMinionWithSpecifiedId()
    {
        // Arrange
        var expectedMinion = new Minion { MinionId = 1, Name = "Test Minion" };
        _mockMinionRepository.Setup(r => r.GetById(1)).Returns(expectedMinion);

        // Act
        var result = _sut.GetMinionById(1);

        // Assert
        result.Should().BeEquivalentTo(expectedMinion);
        _mockMinionRepository.Verify(r => r.GetById(1), Times.Once());
    }

    [Fact]
    public void CreateMinion_InsertsMinion()
    {
        // Arrange
        var minion = new Minion { Name = "New Minion" };
        _mockMinionRepository.Setup(r => r.Insert(It.IsAny<Minion>()));

        // Act
        _sut.CreateMinion(minion);

        // Assert
        _mockMinionRepository.Verify(r => r.Insert(minion), Times.Once());
    }

    [Fact]
    public void UpdateMinion_UpdatesMinion()
    {
        // Arrange
        var minion = new Minion { MinionId = 1, Name = "Updated Minion" };
        _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

        // Act
        _sut.UpdateMinion(minion);

        // Assert
        _mockMinionRepository.Verify(r => r.Update(minion), Times.Once());
    }

    [Fact]
    public void DeleteMinion_DeletesMinion()
    {
        // Arrange
        _mockMinionRepository.Setup(r => r.Delete(1));

        // Act
        _sut.DeleteMinion(1);

        // Assert
        _mockMinionRepository.Verify(r => r.Delete(1), Times.Once());
    }

    #endregion

    #region Business Query Tests

    [Fact]
    public void GetMinionMoodCounts_ReturnsCorrectCounts()
    {
        // Arrange
        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, LoyaltyScore = 85 }, // Happy
                new Minion { MinionId = 2, LoyaltyScore = 75 }, // Happy
                new Minion { MinionId = 3, LoyaltyScore = 55 }, // Grumpy
                new Minion { MinionId = 4, LoyaltyScore = 25 }, // Betrayal
                new Minion { MinionId = 5, LoyaltyScore = 30 }  // Betrayal
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

        // Act
        var result = _sut.GetMinionMoodCounts();

        // Assert
        result["Happy"].Should().Be(2);
        result["Grumpy"].Should().Be(1);
        result["Betrayal"].Should().Be(2);
    }

    [Fact]
    public void GetLowLoyaltyMinions_ReturnsOnlyLowLoyaltyMinions()
    {
        // Arrange
        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, LoyaltyScore = 85 },
                new Minion { MinionId = 2, LoyaltyScore = 35 }, // Low
                new Minion { MinionId = 3, LoyaltyScore = 25 }  // Low
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

        // Act
        var result = _sut.GetLowLoyaltyMinions().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.LoyaltyScore < AppSettings.Instance.LowLoyaltyThreshold).Should().BeTrue();
    }

    [Fact]
    public void CalculateTotalSalaryCosts_SumsAllSalaryDemands()
    {
        // Arrange
        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, SalaryDemand = 5000m },
                new Minion { MinionId = 2, SalaryDemand = 7000m },
                new Minion { MinionId = 3, SalaryDemand = 3000m }
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

        // Act
        var result = _sut.CalculateTotalSalaryCosts();

        // Assert
        result.Should().Be(15000m);
    }

    [Fact]
    public void GetMinionsBySpecialty_FiltersCorrectly()
    {
        // Arrange
        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, Specialty = "Combat" },
                new Minion { MinionId = 3, Specialty = "Hacking" }
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

        // Act
        var result = _sut.GetMinionsBySpecialty("Hacking").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.Specialty == "Hacking").Should().BeTrue();
    }

    [Fact]
    public void GetMinionsForScheme_FiltersCorrectly()
    {
        // Arrange
        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1 },
                new Minion { MinionId = 2, CurrentSchemeId = 2 },
                new Minion { MinionId = 3, CurrentSchemeId = 1 }
            };
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

        // Act
        var result = _sut.GetMinionsForScheme(1).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.CurrentSchemeId == 1).Should().BeTrue();
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public void UpdateMinionLoyalty_WithNullMinion_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.UpdateMinionLoyalty(null!, 5000m));
    }

    [Fact]
    public void UpdateMinionMood_WithNullMinion_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.UpdateMinionMood(null!));
    }

    #endregion
}