using FluentAssertions;
using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.UnitTests.Services;

/// <summary>
/// Unit tests for SchemeService business logic
/// Tests BR-S-001 (Success Likelihood Calculation), BR-S-002 (Budget Enforcement), and status transitions
/// </summary>
public class SchemeServiceTests
{
    private readonly Mock<ISchemeRepository> _mockSchemeRepository;
    private readonly Mock<IMinionRepository> _mockMinionRepository;
    private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
    private readonly SchemeService _sut;

    public SchemeServiceTests()
    {
        _mockSchemeRepository = new Mock<ISchemeRepository>();
        _mockMinionRepository = new Mock<IMinionRepository>();
        _mockEquipmentRepository = new Mock<IEquipmentRepository>();
        _sut = new SchemeService(
            _mockSchemeRepository.Object,
            _mockMinionRepository.Object,
            _mockEquipmentRepository.Object);
    }

    #region BR-S-001: Success Likelihood Calculation Tests

    [Fact]
    public void CalculateSuccessLikelihood_BareMinimum_Returns45Percent()
    {
        // Arrange - 1 matching minion, no equipment, no penalties
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>();

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (1 matching * 10) + (0 equipment * 5) - 0 - 15 (resource penalty) - 0 = 45
        result.Should().Be(45);
    }

    [Fact]
    public void CalculateSuccessLikelihood_WellResourced_Returns100Percent()
    {
        // Arrange - 3 matching minions, 4 equipment, no penalties
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 3, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 80 },
                new Equipment { EquipmentId = 3, AssignedToSchemeId = 1, Condition = 60 },
                new Equipment { EquipmentId = 4, AssignedToSchemeId = 1, Condition = 50 }
            };

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (3 * 10) + (4 * 5) + 0 + 0 + 0 = 100
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateSuccessLikelihood_OverBudget_Applies20PercentPenalty()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 150000m, // Over budget
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 80 }
            };

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (2 * 10) + (2 * 5) - 20 (budget) + 0 + 0 = 60
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateSuccessLikelihood_FailedDeadline_Applies25PercentPenalty()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(-10) // Past deadline
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 80 }
            };

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (2 * 10) + (2 * 5) + 0 + 0 - 25 (timeline) = 55
        result.Should().Be(55);
    }

    [Fact]
    public void CalculateSuccessLikelihood_CompleteFailure_Returns0Percent()
    {
        // Arrange - No minions, no equipment, over budget, past deadline
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 150000m,
            TargetCompletionDate = DateTime.Now.AddDays(-10)
        };

        var minions = new List<Minion>();
        var equipment = new List<Equipment>();

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + 0 + 0 - 20 (budget) - 15 (resource) - 25 (timeline) = -10, clamped to 0
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateSuccessLikelihood_NonMatchingMinions_DoNotProvideBonus()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Combat" }, // Wrong specialty
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }  // Matching
            };

        var equipment = new List<Equipment>();

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (1 matching * 10) + 0 + 0 + 0 (has 2 minions, 1 matching) + 0 = 60
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateSuccessLikelihood_LowConditionEquipment_DoesNotCount()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 }, // Counts
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 40 }   // Too low
            };

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (2 * 10) + (1 * 5) + 0 + 0 + 0 = 75
        result.Should().Be(75);
    }

    [Fact]
    public void CalculateSuccessLikelihood_ClampsToMaximum100()
    {
        // Arrange - Lots of bonuses
        var scheme = new EvilScheme
        {
            SchemeId = 1,
            RequiredSpecialty = "Hacking",
            Budget = 100000m,
            CurrentSpending = 50000m,
            TargetCompletionDate = DateTime.Now.AddDays(30)
        };

        var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 3, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 4, CurrentSchemeId = 1, Specialty = "Hacking" },
                new Minion { MinionId = 5, CurrentSchemeId = 1, Specialty = "Hacking" }
            };

        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 3, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 4, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 5, AssignedToSchemeId = 1, Condition = 100 }
            };

        _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateSuccessLikelihood(scheme);

        // Assert
        // Base 50 + (5 * 10) + (5 * 5) + 0 + 0 + 0 = 125, clamped to 100
        result.Should().Be(100);
    }

    #endregion

    #region BR-S-002: Budget Enforcement Tests

    [Fact]
    public void IsSchemeOverBudget_WhenOverBudget_ReturnsTrue()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Budget = 100000m,
            CurrentSpending = 150000m
        };

        // Act
        var result = _sut.IsSchemeOverBudget(scheme);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSchemeOverBudget_WhenUnderBudget_ReturnsFalse()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Budget = 100000m,
            CurrentSpending = 50000m
        };

        // Act
        var result = _sut.IsSchemeOverBudget(scheme);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSchemeOverBudget_WhenExactlyAtBudget_ReturnsFalse()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Budget = 100000m,
            CurrentSpending = 100000m
        };

        // Act
        var result = _sut.IsSchemeOverBudget(scheme);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Status Filtering Tests

    [Fact]
    public void GetSchemesByStatus_FiltersCorrectly()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Active" },
                new EvilScheme { SchemeId = 2, Status = "Planning" },
                new EvilScheme { SchemeId = 3, Status = "Active" }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.GetSchemesByStatus("Active").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.Status == "Active").Should().BeTrue();
    }

    [Fact]
    public void GetActiveSchemes_ReturnsOnlyActiveSchemes()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Active" },
                new EvilScheme { SchemeId = 2, Status = "Completed" },
                new EvilScheme { SchemeId = 3, Status = "Active" }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.GetActiveSchemes().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.Status == "Active").Should().BeTrue();
    }

    [Fact]
    public void GetOverBudgetSchemes_ReturnsOnlyOverBudgetSchemes()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Budget = 100000m, CurrentSpending = 150000m },
                new EvilScheme { SchemeId = 2, Budget = 100000m, CurrentSpending = 50000m },
                new EvilScheme { SchemeId = 3, Budget = 100000m, CurrentSpending = 120000m }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.GetOverBudgetSchemes().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.CurrentSpending > s.Budget).Should().BeTrue();
    }

    [Fact]
    public void GetOverdueSchemes_ReturnsOnlyOverdueSchemes()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, TargetCompletionDate = DateTime.Now.AddDays(-10), Status = "Active" },
                new EvilScheme { SchemeId = 2, TargetCompletionDate = DateTime.Now.AddDays(10), Status = "Active" },
                new EvilScheme { SchemeId = 3, TargetCompletionDate = DateTime.Now.AddDays(-5), Status = "Completed" }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.GetOverdueSchemes().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].SchemeId.Should().Be(1);
    }

    #endregion

    #region Business Calculation Tests

    [Fact]
    public void CalculateAverageSuccessLikelihood_WithActiveSchemes_ReturnsAverage()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Active", RequiredSpecialty = "Hacking", Budget = 100000m, CurrentSpending = 50000m, TargetCompletionDate = DateTime.Now.AddDays(30) },
                new EvilScheme { SchemeId = 2, Status = "Active", RequiredSpecialty = "Combat", Budget = 100000m, CurrentSpending = 50000m, TargetCompletionDate = DateTime.Now.AddDays(30) },
                new EvilScheme { SchemeId = 3, Status = "Completed", RequiredSpecialty = "Hacking", Budget = 100000m, CurrentSpending = 50000m, TargetCompletionDate = DateTime.Now.AddDays(30) }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);
        _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

        // Act
        var result = _sut.CalculateAverageSuccessLikelihood();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateAverageSuccessLikelihood_WithNoActiveSchemes_ReturnsZero()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Completed" }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.CalculateAverageSuccessLikelihood();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("Hacking", true)]
    [InlineData("Explosives", true)]
    [InlineData("InvalidSpecialty", false)]
    [InlineData("", false)]
    public void IsValidSpecialty_ValidatesCorrectly(string? specialty, bool expectedResult)
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

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(0, false)]
    [InlineData(11, false)]
    [InlineData(-1, false)]
    public void IsValidSkillLevel_ValidatesCorrectly(int skillLevel, bool expectedResult)
    {
        // Act
        var result = _sut.IsValidSkillLevel(skillLevel);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public void CalculateSuccessLikelihood_WithNullScheme_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.CalculateSuccessLikelihood(null!));
    }

    [Fact]
    public void IsSchemeOverBudget_WithNullScheme_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.IsSchemeOverBudget(null!));
    }

    #endregion

    #region CRUD Operation Tests

    [Fact]
    public void GetAllSchemes_ReturnsAllSchemes()
    {
        // Arrange
        var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1 },
                new EvilScheme { SchemeId = 2 }
            };
        _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

        // Act
        var result = _sut.GetAllSchemes();

        // Assert
        result.Should().BeEquivalentTo(schemes);
    }

    [Fact]
    public void CreateScheme_InsertsScheme()
    {
        // Arrange
        var scheme = new EvilScheme { Name = "New Scheme" };

        // Act
        _sut.CreateScheme(scheme);

        // Assert
        _mockSchemeRepository.Verify(r => r.Insert(scheme), Times.Once());
    }

    #endregion
}