using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;
using FluentAssertions;

namespace VillainLairManager.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for EquipmentRepository
/// Tests actual database operations with a test database
/// </summary>
public class EquipmentRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private readonly EquipmentRepository _repository;

    public EquipmentRepositoryIntegrationTests()
    {
        _repository = new EquipmentRepository(Context);
    }

    [Fact]
    public void Insert_And_GetById_ReturnsInsertedEquipment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Laser Gun",
            Category = "Weapon",
            PurchasePrice = 50000m,
            Condition = 100,
            MaintenanceCost = 7500m,
            LastMaintenanceDate = DateTime.Now.AddMonths(-2),
            StoredAtBaseId = 1
        };

        // Act
        _repository.Insert(equipment);
        var retrieved = _repository.GetById(equipment.EquipmentId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Laser Gun");
        retrieved.Category.Should().Be("Weapon");
        retrieved.PurchasePrice.Should().Be(50000m);
        retrieved.Condition.Should().Be(100);
    }

    [Fact]
    public void GetAll_ReturnsAllEquipment()
    {
        // Arrange
        var equipment1 = new Equipment { Name = "Equipment 1", Category = "Weapon", PurchasePrice = 10000m, Condition = 100 };
        var equipment2 = new Equipment { Name = "Equipment 2", Category = "Vehicle", PurchasePrice = 50000m, Condition = 80 };
        var equipment3 = new Equipment { Name = "Equipment 3", Category = "Gadget", PurchasePrice = 5000m, Condition = 60 };

        _repository.Insert(equipment1);
        _repository.Insert(equipment2);
        _repository.Insert(equipment3);

        // Act
        var allEquipment = _repository.GetAll().ToList();

        // Assert
        allEquipment.Should().HaveCount(3);
        allEquipment.Should().Contain(e => e.Name == "Equipment 1");
        allEquipment.Should().Contain(e => e.Name == "Equipment 2");
        allEquipment.Should().Contain(e => e.Name == "Equipment 3");
    }

    [Fact]
    public void Update_ModifiesExistingEquipment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Original Equipment",
            Category = "Weapon",
            PurchasePrice = 10000m,
            Condition = 100,
            MaintenanceCost = 1500m
        };
        _repository.Insert(equipment);

        // Act
        equipment.Condition = 75;
        equipment.MaintenanceCost = 1800m;
        equipment.AssignedToSchemeId = 5;
        _repository.Update(equipment);

        var updated = _repository.GetById(equipment.EquipmentId);

        // Assert
        updated.Should().NotBeNull();
        updated!.Condition.Should().Be(75);
        updated.MaintenanceCost.Should().Be(1800m);
        updated.AssignedToSchemeId.Should().Be(5);
    }

    [Fact]
    public void Delete_RemovesEquipment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "To Be Deleted",
            Category = "Gadget",
            PurchasePrice = 2000m,
            Condition = 50
        };
        _repository.Insert(equipment);
        var equipmentId = equipment.EquipmentId;

        // Act
        _repository.Delete(equipmentId);
        var deleted = _repository.GetById(equipmentId);

        // Assert
        deleted.Should().BeNull();
    }

    [Fact]
    public void Update_Condition_SimulatesDegradation()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Degrading Equipment",
            Category = "Weapon",
            PurchasePrice = 20000m,
            Condition = 100,
            AssignedToSchemeId = 1,
            LastMaintenanceDate = DateTime.Now.AddMonths(-3)
        };
        _repository.Insert(equipment);

        // Act - Simulate degradation
        equipment.Condition = 85;
        _repository.Update(equipment);

        var updated = _repository.GetById(equipment.EquipmentId);

        // Assert
        updated.Should().NotBeNull();
        updated!.Condition.Should().Be(85);
    }

    [Fact]
    public void Update_Maintenance_RestoresConditionAndUpdatesDate()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Maintenance Test",
            Category = "Vehicle",
            PurchasePrice = 100000m,
            Condition = 45,
            LastMaintenanceDate = DateTime.Now.AddMonths(-6)
        };
        _repository.Insert(equipment);

        // Act - Simulate maintenance
        equipment.Condition = 100;
        equipment.LastMaintenanceDate = DateTime.Now;
        equipment.MaintenanceCost = 15000m;
        _repository.Update(equipment);

        var updated = _repository.GetById(equipment.EquipmentId);

        // Assert
        updated.Should().NotBeNull();
        updated!.Condition.Should().Be(100);
        updated.LastMaintenanceDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        updated.MaintenanceCost.Should().Be(15000m);
    }

    [Fact]
    public void Insert_DoomsdayDevice_WithHigherMaintenanceCost()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Death Ray",
            Category = "Doomsday Device",
            PurchasePrice = 1000000m,
            Condition = 100,
            MaintenanceCost = 300000m, // 30% for doomsday devices
            LastMaintenanceDate = DateTime.Now
        };

        // Act
        _repository.Insert(equipment);
        var retrieved = _repository.GetById(equipment.EquipmentId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Category.Should().Be("Doomsday Device");
        retrieved.MaintenanceCost.Should().Be(300000m);
    }

    [Fact]
    public void Update_AssignToScheme_PersistsAssignment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Assignment Test",
            Category = "Weapon",
            PurchasePrice = 15000m,
            Condition = 90,
            AssignedToSchemeId = null
        };
        _repository.Insert(equipment);

        // Act - Assign to scheme
        equipment.AssignedToSchemeId = 3;
        _repository.Update(equipment);

        var updated = _repository.GetById(equipment.EquipmentId);

        // Assert
        updated.Should().NotBeNull();
        updated!.AssignedToSchemeId.Should().Be(3);
    }

    [Fact]
    public void Update_UnassignFromScheme_ClearsAssignment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Unassignment Test",
            Category = "Vehicle",
            PurchasePrice = 75000m,
            Condition = 85,
            AssignedToSchemeId = 2
        };
        _repository.Insert(equipment);

        // Act - Unassign from scheme
        equipment.AssignedToSchemeId = null;
        _repository.Update(equipment);

        var updated = _repository.GetById(equipment.EquipmentId);

        // Assert
        updated.Should().NotBeNull();
        updated!.AssignedToSchemeId.Should().BeNull();
    }

    [Fact]
    public void Insert_WithStorageLocation_PersistsBaseId()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Storage Test",
            Category = "Gadget",
            PurchasePrice = 8000m,
            Condition = 100,
            StoredAtBaseId = 5
        };

        // Act
        _repository.Insert(equipment);
        var retrieved = _repository.GetById(equipment.EquipmentId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.StoredAtBaseId.Should().Be(5);
    }

    [Fact]
    public void GetAll_AfterMultipleOperations_ReturnsAccurateCount()
    {
        // Arrange
        var equipment1 = new Equipment { Name = "E1", Category = "Weapon", PurchasePrice = 10000m, Condition = 100 };
        var equipment2 = new Equipment { Name = "E2", Category = "Vehicle", PurchasePrice = 50000m, Condition = 80 };
        var equipment3 = new Equipment { Name = "E3", Category = "Gadget", PurchasePrice = 5000m, Condition = 60 };

        _repository.Insert(equipment1);
        _repository.Insert(equipment2);
        _repository.Insert(equipment3);

        // Act - Delete one
        _repository.Delete(equipment2.EquipmentId);
        var remaining = _repository.GetAll().ToList();

        // Assert
        remaining.Should().HaveCount(2);
        remaining.Should().Contain(e => e.Name == "E1");
        remaining.Should().Contain(e => e.Name == "E3");
        remaining.Should().NotContain(e => e.Name == "E2");
    }
}