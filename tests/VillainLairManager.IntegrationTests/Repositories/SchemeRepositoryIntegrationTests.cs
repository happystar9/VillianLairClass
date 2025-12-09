using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;
using FluentAssertions;

namespace VillainLairManager.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for SchemeRepository
/// Tests actual database operations with a test database
/// </summary>
public class SchemeRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private readonly SchemeRepository _repository;

    public SchemeRepositoryIntegrationTests()
    {
        _repository = new SchemeRepository(Context);
    }

    [Fact]
    public void Insert_And_GetById_ReturnsInsertedScheme()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Name = "World Domination",
            Description = "Take over the world",
            Budget = 1000000m,
            CurrentSpending = 250000m,
            RequiredSkillLevel = 8,
            RequiredSpecialty = "Hacking",
            Status = "Active",
            StartDate = DateTime.Now.AddDays(-30),
            TargetCompletionDate = DateTime.Now.AddDays(60),
            DiabolicalRating = 9,
            SuccessLikelihood = 75
        };

        // Act
        _repository.Insert(scheme);
        var retrieved = _repository.GetById(scheme.SchemeId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("World Domination");
        retrieved.Budget.Should().Be(1000000m);
        retrieved.CurrentSpending.Should().Be(250000m);
        retrieved.RequiredSpecialty.Should().Be("Hacking");
        retrieved.Status.Should().Be("Active");
    }

    [Fact]
    public void GetAll_ReturnsAllSchemes()
    {
        // Arrange
        var scheme1 = new EvilScheme { Name = "Scheme 1", Budget = 100000m, CurrentSpending = 50000m, RequiredSkillLevel = 5, RequiredSpecialty = "Combat", Status = "Planning", TargetCompletionDate = DateTime.Now.AddDays(30), Description = "A combat-focused scheme", DiabolicalRating = 5 };
        var scheme2 = new EvilScheme { Name = "Scheme 2", Budget = 200000m, CurrentSpending = 100000m, RequiredSkillLevel = 7, RequiredSpecialty = "Hacking", Status = "Active", TargetCompletionDate = DateTime.Now.AddDays(60), Description = "A hacking-focused scheme", DiabolicalRating = 7 };
        var scheme3 = new EvilScheme { Name = "Scheme 3", Budget = 300000m, CurrentSpending = 150000m, RequiredSkillLevel = 9, RequiredSpecialty = "Engineering", Status = "Completed", TargetCompletionDate = DateTime.Now.AddDays(-10), Description = "An engineering-focused scheme", DiabolicalRating = 9 };

        _repository.Insert(scheme1);
        _repository.Insert(scheme2);
        _repository.Insert(scheme3);

        // Act
        var allSchemes = _repository.GetAll().ToList();

        // Assert
        allSchemes.Should().HaveCount(3);
        allSchemes.Should().Contain(s => s.Name == "Scheme 1");
        allSchemes.Should().Contain(s => s.Name == "Scheme 2");
        allSchemes.Should().Contain(s => s.Name == "Scheme 3");
    }

    [Fact]
    public void Update_ModifiesExistingScheme()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Name = "Original Scheme",
            Budget = 100000m,
            CurrentSpending = 0m,
            RequiredSkillLevel = 5,
            RequiredSpecialty = "Combat",
            Status = "Planning",
            TargetCompletionDate = DateTime.Now.AddDays(30),
            Description = "A description of the scheme",
            DiabolicalRating = 5
        };
        _repository.Insert(scheme);

        // Act
        scheme.Status = "Active";
        scheme.CurrentSpending = 25000m;
        scheme.SuccessLikelihood = 60;
        _repository.Update(scheme);

        var updated = _repository.GetById(scheme.SchemeId);

        // Assert
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Active");
        updated.CurrentSpending.Should().Be(25000m);
        updated.SuccessLikelihood.Should().Be(60);
    }

    [Fact]
    public void Delete_RemovesScheme()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Name = "To Be Deleted",
            Budget = 50000m,
            CurrentSpending = 0m,
            RequiredSkillLevel = 3,
            RequiredSpecialty = "Piloting",
            Status = "Planning",
            TargetCompletionDate = DateTime.Now.AddDays(15),
            Description = "A description of the scheme",
            DiabolicalRating = 3
        };
        _repository.Insert(scheme);
        var schemeId = scheme.SchemeId;

        // Act
        _repository.Delete(schemeId);
        var deleted = _repository.GetById(schemeId);

        // Assert
        deleted.Should().BeNull();
    }

    [Fact]
    public void Update_BudgetSpending_CalculatesOverBudget()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Name = "Budget Test",
            Budget = 100000m,
            CurrentSpending = 50000m,
            RequiredSkillLevel = 5,
            RequiredSpecialty = "Hacking",
            Status = "Active",
            TargetCompletionDate = DateTime.Now.AddDays(30),
            Description = "A description of the scheme",
            DiabolicalRating = 5
        };
        _repository.Insert(scheme);

        // Act - Exceed budget
        scheme.CurrentSpending = 150000m;
        _repository.Update(scheme);

        var updated = _repository.GetById(scheme.SchemeId);

        // Assert
        updated.Should().NotBeNull();
        updated!.CurrentSpending.Should().BeGreaterThan(updated.Budget);
    }

    [Fact]
    public void Insert_WithDates_PersistsDatesCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1, 9, 0, 0);
        var targetDate = new DateTime(2025, 12, 31, 17, 0, 0);

        var scheme = new EvilScheme
        {
            Name = "Date Test",
            Budget = 100000m,
            CurrentSpending = 0m,
            RequiredSkillLevel = 5,
            RequiredSpecialty = "Hacking",
            Status = "Planning",
            StartDate = startDate,
            TargetCompletionDate = targetDate,
            Description = "A description of the scheme",
            DiabolicalRating = 5
        };

        // Act
        _repository.Insert(scheme);
        var retrieved = _repository.GetById(scheme.SchemeId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.StartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        retrieved.TargetCompletionDate.Should().BeCloseTo(targetDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_SuccessLikelihood_ReflectsBusinessLogicChanges()
    {
        // Arrange
        var scheme = new EvilScheme
        {
            Name = "Success Test",
            Budget = 100000m,
            CurrentSpending = 50000m,
            RequiredSkillLevel = 5,
            RequiredSpecialty = "Hacking",
            Status = "Active",
            TargetCompletionDate = DateTime.Now.AddDays(30),
            SuccessLikelihood = 50,
            Description = "A description of the scheme",
            DiabolicalRating = 5
        };
        _repository.Insert(scheme);

        // Act - Simulate success likelihood recalculation
        scheme.SuccessLikelihood = 85;
        _repository.Update(scheme);

        var updated = _repository.GetById(scheme.SchemeId);

        // Assert
        updated.Should().NotBeNull();
        updated!.SuccessLikelihood.Should().Be(85);
    }
}