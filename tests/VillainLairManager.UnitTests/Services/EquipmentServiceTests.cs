using FluentAssertions;
using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.UnitTests.Services;

/// <summary>
/// Unit tests for EquipmentService business logic
/// Tests BR-E-001 (Condition Degradation) and BR-E-002 (Maintenance Operations)
/// </summary>
public class EquipmentServiceTests
{
    private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
    private readonly Mock<ISchemeRepository> _mockSchemeRepository;
    private readonly EquipmentService _sut;

    public EquipmentServiceTests()
    {
        _mockEquipmentRepository = new Mock<IEquipmentRepository>();
        _mockSchemeRepository = new Mock<ISchemeRepository>();
        _sut = new EquipmentService(_mockEquipmentRepository.Object, _mockSchemeRepository.Object);
    }

    #region BR-E-001: Condition Degradation Tests

    [Fact]
    public void DegradeEquipmentCondition_WhenAssignedToActiveScheme_DegradesBy5Percent()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 100,
            AssignedToSchemeId = 1,
            LastMaintenanceDate = DateTime.Now.AddMonths(-1)
        };

        var scheme = new EvilScheme
        {
            SchemeId = 1,
            Status = AppSettings.Instance.StatusActive
        };

        _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        _sut.DegradeEquipmentCondition(equipment);

        // Assert
        equipment.Condition.Should().Be(95);
        _mockEquipmentRepository.Verify(r => r.Update(equipment), Times.Once());
    }

    [Fact]
    public void DegradeEquipmentCondition_WhenNotAssigned_DoesNotDegrade()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 100,
            AssignedToSchemeId = null
        };

        // Act
        _sut.DegradeEquipmentCondition(equipment);

        // Assert
        equipment.Condition.Should().Be(100);
        _mockEquipmentRepository.Verify(r => r.Update(It.IsAny<Equipment>()), Times.Never());
    }

    [Fact]
    public void DegradeEquipmentCondition_WhenSchemeNotActive_DoesNotDegrade()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 100,
            AssignedToSchemeId = 1
        };

        var scheme = new EvilScheme
        {
            SchemeId = 1,
            Status = "Completed"
        };

        _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);

        // Act
        _sut.DegradeEquipmentCondition(equipment);

        // Assert
        equipment.Condition.Should().Be(100);
        _mockEquipmentRepository.Verify(r => r.Update(It.IsAny<Equipment>()), Times.Never());
    }

    [Fact]
    public void DegradeEquipmentCondition_CannotGoBelowZero()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 3,
            AssignedToSchemeId = 1,
            LastMaintenanceDate = DateTime.Now.AddMonths(-1)
        };

        var scheme = new EvilScheme
        {
            SchemeId = 1,
            Status = AppSettings.Instance.StatusActive
        };

        _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        _sut.DegradeEquipmentCondition(equipment);

        // Assert
        equipment.Condition.Should().Be(0);
    }

    #endregion

    #region BR-E-002: Maintenance Operations Tests

    [Fact]
    public void PerformMaintenance_RestoresConditionTo100()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 50,
            PurchasePrice = 10000m,
            Category = "Weapon"
        };

        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        var cost = _sut.PerformMaintenance(equipment);

        // Assert
        equipment.Condition.Should().Be(100);
        equipment.LastMaintenanceDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        _mockEquipmentRepository.Verify(r => r.Update(equipment), Times.Once());
    }

    [Fact]
    public void PerformMaintenance_ForNormalEquipment_Costs15PercentOfPurchasePrice()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 50,
            PurchasePrice = 10000m,
            Category = "Weapon"
        };

        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        var cost = _sut.PerformMaintenance(equipment);

        // Assert
        cost.Should().Be(1500m); // 15% of 10000
    }

    [Fact]
    public void PerformMaintenance_ForDoomsdayDevice_Costs30PercentOfPurchasePrice()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 50,
            PurchasePrice = 100000m,
            Category = "Doomsday Device"
        };

        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        var cost = _sut.PerformMaintenance(equipment);

        // Assert
        cost.Should().Be(30000m); // 30% of 100000
    }

    [Fact]
    public void PerformMaintenance_UpdatesLastMaintenanceDate()
    {
        // Arrange
        var equipment = new Equipment
        {
            EquipmentId = 1,
            Condition = 50,
            PurchasePrice = 10000m,
            Category = "Weapon",
            LastMaintenanceDate = DateTime.Now.AddMonths(-6)
        };

        _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

        // Act
        _sut.PerformMaintenance(equipment);

        // Assert
        equipment.LastMaintenanceDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Equipment Status Tests

    [Fact]
    public void IsEquipmentOperational_WhenConditionAboveMinimum_ReturnsTrue()
    {
        // Arrange
        var equipment = new Equipment { Condition = 80 };

        // Act
        var result = _sut.IsEquipmentOperational(equipment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEquipmentOperational_WhenConditionBelowMinimum_ReturnsFalse()
    {
        // Arrange
        var equipment = new Equipment { Condition = 40 };

        // Act
        var result = _sut.IsEquipmentOperational(equipment);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquipmentOperational_WhenConditionExactlyAtMinimum_ReturnsTrue()
    {
        // Arrange
        var equipment = new Equipment { Condition = AppSettings.Instance.MinEquipmentCondition };

        // Act
        var result = _sut.IsEquipmentOperational(equipment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEquipmentBroken_WhenConditionBelowBrokenThreshold_ReturnsTrue()
    {
        // Arrange
        var equipment = new Equipment { Condition = 15 };

        // Act
        var result = _sut.IsEquipmentBroken(equipment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEquipmentBroken_WhenConditionAboveBrokenThreshold_ReturnsFalse()
    {
        // Arrange
        var equipment = new Equipment { Condition = 50 };

        // Act
        var result = _sut.IsEquipmentBroken(equipment);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Filtering and Query Tests

    [Fact]
    public void GetBrokenEquipment_ReturnsOnlyBrokenEquipment()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Condition = 15 }, // Broken
                new Equipment { EquipmentId = 2, Condition = 50 },
                new Equipment { EquipmentId = 3, Condition = 10 }  // Broken
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetBrokenEquipment().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Condition < AppSettings.Instance.BrokenEquipmentCondition).Should().BeTrue();
    }

    [Fact]
    public void CalculateTotalMaintenanceCosts_SumsAllMaintenanceCosts()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, MaintenanceCost = 1500m },
                new Equipment { EquipmentId = 2, MaintenanceCost = 2000m },
                new Equipment { EquipmentId = 3, MaintenanceCost = 500m }
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.CalculateTotalMaintenanceCosts();

        // Assert
        result.Should().Be(4000m);
    }

    [Fact]
    public void GetEquipmentByCategory_FiltersCorrectly()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Category = "Weapon" },
                new Equipment { EquipmentId = 2, Category = "Vehicle" },
                new Equipment { EquipmentId = 3, Category = "Weapon" }
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetEquipmentByCategory("Weapon").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Category == "Weapon").Should().BeTrue();
    }

    [Fact]
    public void GetEquipmentForScheme_ReturnsOnlyAssignedEquipment()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 2 },
                new Equipment { EquipmentId = 3, AssignedToSchemeId = 1 }
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetEquipmentForScheme(1).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.AssignedToSchemeId == 1).Should().BeTrue();
    }

    [Fact]
    public void GetOperationalEquipmentForScheme_FiltersOperationalOnly()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 80 },  // Operational
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 30 },  // Not operational
                new Equipment { EquipmentId = 3, AssignedToSchemeId = 1, Condition = 60 }   // Operational
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetOperationalEquipmentForScheme(1).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Condition >= AppSettings.Instance.MinEquipmentCondition).Should().BeTrue();
    }

    [Fact]
    public void GetEquipmentAtBase_FiltersCorrectly()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, StoredAtBaseId = 1 },
                new Equipment { EquipmentId = 2, StoredAtBaseId = 2 },
                new Equipment { EquipmentId = 3, StoredAtBaseId = 1 }
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetEquipmentAtBase(1).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.StoredAtBaseId == 1).Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("Weapon", true)]
    [InlineData("Vehicle", true)]
    [InlineData("Gadget", true)]
    [InlineData("Doomsday Device", true)]
    [InlineData("InvalidCategory", false)]
    [InlineData("", false)]
    public void IsValidCategory_ValidatesCorrectly(string? category, bool expectedResult)
    {
        // Act
        var result = _sut.IsValidCategory(category!);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsValidCategory_WithNull_ReturnsFalse()
    {
        // Act
        var result = _sut.IsValidCategory(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    public void IsValidCondition_ValidatesCorrectly(int condition, bool expectedResult)
    {
        // Act
        var result = _sut.IsValidCondition(condition);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public void DegradeEquipmentCondition_WithNullEquipment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.DegradeEquipmentCondition(null!));
    }

    [Fact]
    public void PerformMaintenance_WithNullEquipment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.PerformMaintenance(null!));
    }

    [Fact]
    public void IsEquipmentOperational_WithNullEquipment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.IsEquipmentOperational(null!));
    }

    [Fact]
    public void IsEquipmentBroken_WithNullEquipment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.IsEquipmentBroken(null!));
    }

    #endregion

    #region CRUD Operation Tests

    [Fact]
    public void GetAllEquipment_ReturnsAllEquipment()
    {
        // Arrange
        var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1 },
                new Equipment { EquipmentId = 2 }
            };
        _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

        // Act
        var result = _sut.GetAllEquipment();

        // Assert
        result.Should().BeEquivalentTo(equipment);
    }

    [Fact]
    public void CreateEquipment_InsertsEquipment()
    {
        // Arrange
        var equipment = new Equipment { Name = "New Weapon" };

        // Act
        _sut.CreateEquipment(equipment);

        // Assert
        _mockEquipmentRepository.Verify(r => r.Insert(equipment), Times.Once());
    }

    #endregion
}