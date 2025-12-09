# Anti-Patterns Analysis: Villain Lair Manager

## Anti-Pattern 1: Huge Class (Static Singleton DatabaseHelper)

### Location

**File**: `DatabaseHelper.cs`
**Lines**: 1-600 (entire file)

### Description

The `DatabaseHelper` class is a static singleton that violates the Single Responsibility Principle (SRP) by handling:
- Database connection management
- Schema creation
- Data seeding
- CRUD operations for ALL entities (Minions, Schemes, Bases, Equipment)
- Helper queries
- Transaction management (implicitly)

```csharp
// Lines 14-27
public static class DatabaseHelper
{
    private static SQLiteConnection _connection = null;
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized)
            return;

        string dbPath = ConfigManager.DatabasePath;
        _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        _connection.Open();
        _isInitialized = true;
    }
```

### Why It's Problematic

1. **Violates Single Responsibility Principle**: This class has at least 8 distinct responsibilities
2. **Cannot be unit tested**: Static methods and state make mocking impossible
3. **Global state management**: The static `_connection` creates shared mutable state
4. **Tight coupling**: Every part of the application directly depends on this class
5. **No dependency injection**: Cannot swap implementations for testing or different data sources
6. **Concurrency issues**: Single static connection is not thread-safe

### Impact on Testability

- **Unit Testing**: Impossible to mock database operations without refactoring
- **Integration Testing**: All tests must use the same SQLite database
- **Test Isolation**: Tests cannot run in parallel due to shared connection state
- **Cannot test failure scenarios**: No way to inject database errors for testing

### Impact on Maintainability

- **Hard to modify**: Changes to database access affect entire codebase
- **Difficult to add new data sources**: Cannot easily switch from SQLite to SQL Server
- **Poor separation of concerns**: Business logic, data access, and infrastructure mixed
- **Violates Open/Closed Principle**: Must modify this class to add new entity types

### Recommended Solution

```csharp
// Repository pattern with dependency injection
public interface IMinionRepository
{
    List<Minion> GetAll();
    Minion GetById(int id);
    void Insert(Minion minion);
    void Update(Minion minion);
    void Delete(int id);
}

public class SqliteMinionRepository : IMinionRepository
{
    private readonly IDbConnection _connection;
    
    public SqliteMinionRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    // Implementation...
}
```

---

## Anti-Pattern 2: Anemic Domain Models with Scattered Business Logic

### Location

**Models with business logic**:
- `Models/Minion.cs` - Lines 24-65
- `Models/EvilScheme.cs` - Lines 27-73
- `Models/Equipment.cs` - Lines 18-68
- `Models/SecretBase.cs` - Lines 19-32

### Description

The domain models contain properties but also mix in:
- Business logic (calculations, validations)
- Direct database calls
- Static utility methods
- Infrastructure concerns

**Example from Minion.cs (Lines 24-38)**:
```csharp
// Business logic mixed into model (anti-pattern)
public void UpdateMood()
{
    // Business rules embedded in model
    if (this.LoyaltyScore > ConfigManager.HighLoyaltyThreshold)
        this.MoodStatus = ConfigManager.MoodHappy;
    else if (this.LoyaltyScore < ConfigManager.LowLoyaltyThreshold)
        this.MoodStatus = ConfigManager.MoodBetrayal;
    else
        this.MoodStatus = ConfigManager.MoodGrumpy;

    this.LastMoodUpdate = DateTime.Now;

    // Directly accesses database (anti-pattern)
    DatabaseHelper.UpdateMinion(this);
}
```

**Example from EvilScheme.cs (Lines 27-73)**:
```csharp
// Business logic in model (anti-pattern)
// This calculation is also duplicated in forms (major anti-pattern)
public int CalculateSuccessLikelihood()
{
    int baseSuccess = 50;

    // Get assigned minions from database (model accessing database - anti-pattern)
    var assignedMinions = DatabaseHelper.GetAllMinions();
    int matchingMinions = 0;
    int totalMinions = 0;

    foreach (var minion in assignedMinions)
    {
        if (minion.CurrentSchemeId == this.SchemeId)
        {
            totalMinions++;
            if (minion.Specialty == this.RequiredSpecialty)
            {
                matchingMinions++;
            }
        }
    }
    // ... more calculation logic
```

**Example from Equipment.cs (Lines 22-35)**:
```csharp
// Business logic: condition degradation
public void DegradeCondition()
{
    if (AssignedToSchemeId.HasValue)
    {
        // Check if scheme is active
        var scheme = DatabaseHelper.GetSchemeById(AssignedToSchemeId.Value);
        if (scheme != null && scheme.Status == ConfigManager.StatusActive)
        {
            int monthsSinceMaintenance = 1; // Simplified - should calculate from LastMaintenanceDate
            int degradation = monthsSinceMaintenance * ConfigManager.ConditionDegradationRate;
            Condition -= degradation;

            if (Condition < 0) Condition = 0;

            DatabaseHelper.UpdateEquipment(this);
        }
    }
}
```

### Why It's Problematic

1. **Violates Separation of Concerns**: Models shouldn't access database or perform I/O
2. **Hidden dependencies**: `UpdateMood()` has invisible dependency on database
3. **Hard to test**: Cannot test business logic without database
4. **Active Record anti-pattern**: Models manage their own persistence
5. **Poor cohesion**: Models mix data representation with behavior

### Impact on Testability

- **Cannot unit test business logic**: Requires full database setup
- **Tight coupling to infrastructure**: Models depend on `DatabaseHelper`
- **Cannot mock dependencies**: Static calls cannot be intercepted
- **Side effects**: Methods modify database state unpredictably

### Impact on Maintainability

- **Difficult to change persistence**: Models tied to SQLite implementation
- **Business rules scattered**: Logic spread across Models, Forms, and DatabaseHelper
- **Violation of DDD principles**: Domain models should be persistence-ignorant
- **Poor discoverability**: Hard to find where business rules are implemented

### Recommended Solution

```csharp
// Pure domain model (no infrastructure dependencies)
public class Minion
{
    public int MinionId { get; set; }
    public string Name { get; set; }
    public int SkillLevel { get; set; }
    public string Specialty { get; set; }
    public int LoyaltyScore { get; private set; }
    public string MoodStatus { get; private set; }
    
    // Business logic only - no database calls
    public void UpdateMood()
    {
        MoodStatus = LoyaltyScore switch
        {
            > 70 => "Happy",
            < 40 => "Plotting Betrayal",
            _ => "Grumpy"
        };
    }
    
    public void AdjustLoyalty(decimal actualSalary, decimal demandedSalary)
    {
        if (actualSalary >= demandedSalary)
            LoyaltyScore = Math.Min(100, LoyaltyScore + 3);
        else
            LoyaltyScore = Math.Max(0, LoyaltyScore - 5);
            
        UpdateMood();
    }
}

// Business logic in separate service
public class MinionService
{
    private readonly IMinionRepository _repository;
    
    public void ProcessSalaryPayment(Minion minion, decimal payment)
    {
        minion.AdjustLoyalty(payment, minion.SalaryDemand);
        _repository.Update(minion);
    }
}
```

---

## Anti-Pattern 3: Business Logic in UI Layer

### Location

**File**: `Forms/SchemeManagementForm.cs`
**Lines**: 
- 157-165 (Status change event handler)
- 330-360 (Add button with validation)
- 409-450 (Update button with validation)
- 522-603 (Success likelihood calculation in UI)

**File**: `Forms/MinionManagementForm.cs`
**Lines**: 120-170 (Add button handler)

### Description

Business logic, validation rules, and calculations are embedded directly in form event handlers instead of being in a separate business logic layer.

**Example 1 - Status Transition Logic in ComboBox Event (Lines 157-165)**:
```csharp
// Status transition logic in ComboBox event (anti-pattern)
cmbStatus.SelectedIndexChanged += (sender, e) =>
{
    // Business logic in UI event handler (anti-pattern)
    if (cmbStatus.SelectedItem.ToString() == "Active" && dtpStartDate.Value == DateTime.Today)
    {
        dtpStartDate.Value = DateTime.Now;
    }
};
```

**Example 2 - Budget Validation in Button Handler (Lines 330-341)**:
```csharp
// Budget validation in button handler (anti-pattern)
if (numCurrentSpending.Value > numBudget.Value)
{
    var result = MessageBox.Show("Warning: Current spending exceeds budget! This will reduce success likelihood. Continue?", 
        "Budget Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
    if (result != DialogResult.Yes)
        return;
}

// Validation logic in UI (anti-pattern)
if (string.IsNullOrWhiteSpace(txtName.Text))
{
    MessageBox.Show("Scheme name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return;
}
```

**Example 3 - Success Likelihood Calculation Duplicated in UI (Lines 522-603)**:
```csharp
// Success likelihood calculation duplicated in UI (major anti-pattern)
// This is a duplicate of the calculation in EvilScheme.CalculateSuccessLikelihood()
private int CalculateSuccessLikelihoodInUI(EvilScheme scheme)
{
    int baseSuccess = 50;

    // Get assigned minions from database (UI accessing database - anti-pattern)
    var assignedMinions = DatabaseHelper.GetAllMinions();
    int matchingMinions = 0;
    int totalMinions = 0;

    if (scheme != null)
    {
        foreach (var minion in assignedMinions)
        {
            if (minion.CurrentSchemeId == scheme.SchemeId)
            {
                totalMinions++;
                if (minion.Specialty == scheme.RequiredSpecialty)
                {
                    matchingMinions++;
                }
            }
        }
    }

    int minionBonus = matchingMinions * 10;

    // ... 70 more lines of calculation logic duplicated from model
```

**Example 4 - Direct Database Calls from UI (Lines 377-387)**:
```csharp
// Direct database call from event handler (anti-pattern)
DatabaseHelper.InsertScheme(newScheme);

// Reload to get the ID
var schemes = DatabaseHelper.GetAllSchemes();
binding.Clear();
foreach (var s in schemes)
    binding.Add(s);
```

### Why It's Problematic

1. **Violates MVC/MVP pattern**: View contains business logic
2. **Cannot reuse logic**: Business rules locked in UI code
3. **Hard to test**: Requires instantiating WinForms controls
4. **Poor separation of concerns**: UI, validation, and business logic mixed
5. **Duplicate code**: Same validation appears in multiple forms

### Impact on Testability

- **Cannot unit test validations**: Requires UI framework
- **Integration tests only**: Must use UI automation tools
- **No business logic tests**: Logic is not separated from presentation
- **Brittle tests**: Tests break when UI layout changes

### Impact on Maintainability

- **Hard to find business rules**: Scattered across multiple forms
- **Difficult to change validation**: Must update every form
- **Cannot use in other contexts**: Logic tied to WinForms
- **Poor reusability**: Cannot use same logic in Web UI or API

### Recommended Solution

```csharp
// Validation service
public class SchemeValidator
{
    public ValidationResult Validate(EvilScheme scheme)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(scheme.Name))
            errors.Add("Scheme name is required");
            
        if (scheme.CurrentSpending > scheme.Budget)
            errors.Add("Current spending exceeds budget");
            
        if (scheme.TargetCompletionDate <= DateTime.Now)
            errors.Add("Target completion must be in future");
            
        return new ValidationResult(errors);
    }
}

// Business service
public class SchemeService
{
    private readonly ISchemeRepository _repository;
    private readonly SchemeValidator _validator;
    private readonly ISuccessCalculator _calculator;
    
    public Result AddScheme(EvilScheme scheme)
    {
        var validation = _validator.Validate(scheme);
        if (!validation.IsValid)
            return Result.Failure(validation.Errors);
            
        scheme.SuccessLikelihood = _calculator.Calculate(scheme);
        _repository.Insert(scheme);
        return Result.Success();
    }
}

// Form becomes thin (just UI logic)
private void AddButton_Click(object sender, EventArgs e)
{
    var scheme = MapFormToModel();
    var result = _schemeService.AddScheme(scheme);
    
    if (!result.IsSuccess)
    {
        MessageBox.Show(string.Join("\n", result.Errors));
        return;
    }
    
    RefreshGrid();
}
```

---

## Anti-Pattern 4: Duplicate Code (Success Likelihood Calculation)

### Location

**Duplicated in multiple places**:
1. `Models/EvilScheme.cs` - Lines 27-73
2. `Forms/SchemeManagementForm.cs` - Lines 522-603
3. Potentially in `Forms/MainForm.cs` (dashboard statistics)

### Description

The complex success likelihood calculation algorithm is duplicated in at least two places with identical logic, creating a maintenance nightmare.

**Version 1 - In Model (EvilScheme.cs, Lines 27-73)**:
```csharp
public int CalculateSuccessLikelihood()
{
    int baseSuccess = 50;

    // Get assigned minions from database (model accessing database - anti-pattern)
    var assignedMinions = DatabaseHelper.GetAllMinions();
    int matchingMinions = 0;
    int totalMinions = 0;

    foreach (var minion in assignedMinions)
    {
        if (minion.CurrentSchemeId == this.SchemeId)
        {
            totalMinions++;
            if (minion.Specialty == this.RequiredSpecialty)
            {
                matchingMinions++;
            }
        }
    }

    int minionBonus = matchingMinions * 10;

    // Get assigned equipment
    var assignedEquipment = DatabaseHelper.GetAllEquipment();
    int workingEquipmentCount = 0;

    foreach (var equipment in assignedEquipment)
    {
        if (equipment.AssignedToSchemeId == this.SchemeId &&
            equipment.Condition >= ConfigManager.MinEquipmentCondition)
        {
            workingEquipmentCount++;
        }
    }

    int equipmentBonus = workingEquipmentCount * 5;

    // Penalties
    int budgetPenalty = (this.CurrentSpending > this.Budget) ? -20 : 0;
    int resourcePenalty = (totalMinions >= 2 && matchingMinions >= 1) ? 0 : -15;
    int timelinePenalty = (DateTime.Now > this.TargetCompletionDate) ? -25 : 0;

    // Calculate final
    int success = baseSuccess + minionBonus + equipmentBonus + budgetPenalty + resourcePenalty + timelinePenalty;

    // Clamp to 0-100
    if (success < 0) success = 0;
    if (success > 100) success = 100;

    return success;
}
```

**Version 2 - In Form (SchemeManagementForm.cs, Lines 522-603)**:
```csharp
// Success likelihood calculation duplicated in UI (major anti-pattern)
// This is a duplicate of the calculation in EvilScheme.CalculateSuccessLikelihood()
private int CalculateSuccessLikelihoodInUI(EvilScheme scheme)
{
    int baseSuccess = 50;

    // Get assigned minions from database (UI accessing database - anti-pattern)
    var assignedMinions = DatabaseHelper.GetAllMinions();
    int matchingMinions = 0;
    int totalMinions = 0;

    if (scheme != null)
    {
        foreach (var minion in assignedMinions)
        {
            if (minion.CurrentSchemeId == scheme.SchemeId)
            {
                totalMinions++;
                if (minion.Specialty == scheme.RequiredSpecialty)
                {
                    matchingMinions++;
                }
            }
        }
    }

    int minionBonus = matchingMinions * 10;

    // Get assigned equipment
    var assignedEquipment = DatabaseHelper.GetAllEquipment();
    int workingEquipmentCount = 0;

    if (scheme != null)
    {
        foreach (var equipment in assignedEquipment)
        {
            if (equipment.AssignedToSchemeId == scheme.SchemeId && equipment.Condition >= 50)
            {
                workingEquipmentCount++;
            }
        }
    }

    int equipmentBonus = workingEquipmentCount * 5;

    // Penalties
    decimal budget = scheme?.Budget ?? numBudget.Value;
    decimal spending = scheme?.CurrentSpending ?? numCurrentSpending.Value;
    int budgetPenalty = (spending > budget) ? -20 : 0;

    int resourcePenalty = (totalMinions >= 2 && matchingMinions >= 1) ? 0 : -15;

    DateTime targetDate = scheme?.TargetCompletionDate ?? dtpTargetCompletion.Value;
    int timelinePenalty = (DateTime.Now > targetDate) ? -25 : 0;

    // Calculate final
    int success = baseSuccess + minionBonus + equipmentBonus + budgetPenalty + resourcePenalty + timelinePenalty;

    // Clamp to 0-100
    if (success < 0) success = 0;
    if (success > 100) success = 100;

    return success;
}
```

### Why It's Problematic

1. **DRY violation**: Same logic exists in multiple places
2. **Maintenance nightmare**: Bug fixes must be applied everywhere
3. **Inconsistency risk**: Versions can drift and produce different results
4. **Hard to find all occurrences**: No guarantee all copies are updated
5. **Increased cognitive load**: Developers must remember multiple locations

### Impact on Testability

- **Multiple test suites needed**: Each implementation needs separate tests
- **Inconsistent behavior**: Different versions may have subtle differences
- **Test coverage gaps**: Easy to miss testing one of the implementations
- **Regression risk**: Fixing one version may leave others broken

### Impact on Maintainability

- **Change amplification**: Single business rule change requires multiple code edits
- **High defect risk**: Easy to miss updating one copy
- **Code review complexity**: Reviewers must verify all copies are identical
- **Refactoring difficulty**: Must update all locations simultaneously

### Recommended Solution

```csharp
// Single, testable implementation
public class SuccessLikelihoodCalculator
{
    private readonly IMinionRepository _minionRepo;
    private readonly IEquipmentRepository _equipmentRepo;
    
    public int Calculate(EvilScheme scheme)
    {
        const int baseSuccess = 50;
        
        var minions = _minionRepo.GetBySchemeId(scheme.SchemeId);
        var matchingMinions = minions.Count(m => m.Specialty == scheme.RequiredSpecialty);
        
        var equipment = _equipmentRepo.GetBySchemeId(scheme.SchemeId);
        var workingEquipment = equipment.Count(e => e.Condition >= 50);
        
        var bonuses = CalculateBonuses(matchingMinions, workingEquipment);
        var penalties = CalculatePenalties(scheme, minions.Count, matchingMinions);
        
        return Math.Clamp(baseSuccess + bonuses - penalties, 0, 100);
    }
    
    private int CalculateBonuses(int matchingMinions, int workingEquipment)
    {
        return (matchingMinions * 10) + (workingEquipment * 5);
    }
    
    private int CalculatePenalties(EvilScheme scheme, int totalMinions, int matchingMinions)
    {
        int penalty = 0;
        
        if (scheme.CurrentSpending > scheme.Budget)
            penalty += 20;
            
        if (totalMinions < 2 || matchingMinions < 1)
            penalty += 15;
            
        if (DateTime.Now > scheme.TargetCompletionDate)
            penalty += 25;
            
        return penalty;
    }
}
```

---

## Anti-Pattern 5: Magic Strings and Numbers

### Location

**Throughout the codebase**:
- `Utils/ConfigManager.cs` - Lines 1-65 (hardcoded constants)
- `Utils/ValidationHelper.cs` - Lines 10-24 (hardcoded specialty lists)
- `Models/Minion.cs` - Lines 42-47 (hardcoded specialty validation)
- `Forms/SchemeManagementForm.cs` - Lines 155 (status values)

### Description

Magic numbers and strings are scattered throughout the code, with duplicated lists and hardcoded values instead of centralized configuration.

**Example 1 - Hardcoded Specialty Lists (3 different places)**:

```csharp
// In ConfigManager.cs (Lines 29-37)
public static readonly string[] ValidSpecialties = new string[]
{
    "Hacking",
    "Explosives",
    "Disguise",
    "Combat",
    "Engineering",
    "Piloting"
};

// In ValidationHelper.cs (Lines 10-14)
public static bool IsValidSpecialty(string specialty)
{
    // Hardcoded list instead of using ConfigManager (anti-pattern)
    return specialty == "Hacking" || specialty == "Explosives" ||
           specialty == "Disguise" || specialty == "Combat" ||
           specialty == "Engineering" || specialty == "Piloting";
}

// In Minion.cs (Lines 42-47)
public static bool IsValidSpecialty(string specialty)
{
    // Hardcoded list (duplicated from ValidationHelper)
    return specialty == "Hacking" || specialty == "Explosives" ||
           specialty == "Disguise" || specialty == "Combat" ||
           specialty == "Engineering" || specialty == "Piloting";
}
```

**Example 2 - Magic Numbers in Business Logic**:

```csharp
// In ConfigManager.cs (Lines 15-25)
public const int LoyaltyDecayRate = 5;
public const int LoyaltyGrowthRate = 3;
public const int ConditionDegradationRate = 5;
public const decimal MaintenanceCostPercentage = 0.15m;
public const decimal DoomsdayMaintenanceCostPercentage = 0.30m;

// Used directly in calculations without context
// Equipment.cs Line 31
int degradation = monthsSinceMaintenance * ConfigManager.ConditionDegradationRate; // Why 5?

// EvilScheme.cs Line 46
int minionBonus = matchingMinions * 10; // Magic number - why 10?
int equipmentBonus = workingEquipmentCount * 5; // Magic number - why 5?
```

**Example 3 - Status Strings (Lines 20-25)**:

```csharp
// In ConfigManager.cs
public const string StatusPlanning = "Planning";
public const string StatusActive = "Active";
public const string StatusOnHold = "On Hold";
public const string StatusCompleted = "Completed";
public const string StatusFailed = "Failed";

// But in forms, sometimes used as raw strings:
// SchemeManagementForm.cs Line 156
cmbStatus.Items.AddRange(new object[] { "Planning", "Active", "Completed", "Failed", "On Hold" });
```

### Why It's Problematic

1. **Duplicate data**: Same lists exist in multiple places
2. **Inconsistency**: Easy for copies to drift apart
3. **Hard to maintain**: Changing a value requires finding all occurrences
4. **Type safety issues**: Strings are error-prone (typos)
5. **No single source of truth**: Multiple definitions of same concepts

### Impact on Testability

- **Hard to test edge cases**: Magic numbers lack semantic meaning
- **Difficult to parameterize tests**: Values hardcoded in business logic
- **Cannot easily change test scenarios**: Must modify production code
- **Test data maintenance**: Tests must duplicate magic values

### Impact on Maintainability

- **Change amplification**: Adding a specialty requires 3+ code changes
- **Defect risk**: Easy to miss updating one location
- **Poor discoverability**: No central place to see all valid values
- **Refactoring difficulty**: Must find and update all occurrences

### Recommended Solution

```csharp
// Centralized enumerations
public enum Specialty
{
    Hacking,
    Explosives,
    Disguise,
    Combat,
    Engineering,
    Piloting
}

public enum SchemeStatus
{
    Planning,
    Active,
    OnHold,
    Completed,
    Failed
}

// Configuration from file (not hardcoded)
public class BusinessRulesConfig
{
    public int LoyaltyDecayRate { get; set; } = 5;
    public int LoyaltyGrowthRate { get; set; } = 3;
    public int MinionBonusPerMatch { get; set; } = 10;
    public int EquipmentBonusPerItem { get; set; } = 5;
    
    // Load from appsettings.json or configuration file
    public static BusinessRulesConfig Load()
    {
        // Use IConfiguration or config file
    }
}

// Usage with type safety
if (minion.Specialty == Specialty.Hacking)
{
    // Type-safe, refactorable, discoverable
}
```

---

## Anti-Pattern 6: Lack of Error Handling

### Location

**Throughout the codebase**:
- `Program.cs` - Lines 19-26 (no error handling on initialization)
- `DatabaseHelper.cs` - Lines 261-266 (delete without checking dependencies)
- `Forms/SchemeManagementForm.cs` - Most database operations wrapped in generic try/catch

### Description

The application has minimal error handling, and where it exists, it's often too broad or missing critical checks.

**Example 1 - No Error Handling in Program.cs (Lines 19-26)**:
```csharp
// Initialize database - no error handling (anti-pattern)
DatabaseHelper.Initialize();

// Create schema if needed
DatabaseHelper.CreateSchemaIfNotExists();

// Seed data on first run - no check if already seeded (anti-pattern)
DatabaseHelper.SeedInitialData();

Application.Run(new MainForm());
```

**Example 2 - Delete Without Validation (DatabaseHelper.cs, Lines 261-266)**:
```csharp
public static void DeleteMinion(int minionId)
{
    // No error handling - just delete (anti-pattern)
    var command = new SQLiteCommand("DELETE FROM Minions WHERE MinionId = @id", _connection);
    command.Parameters.AddWithValue("@id", minionId);
    command.ExecuteNonQuery();
}

public static void DeleteScheme(int schemeId)
{
    var command = new SQLiteCommand("DELETE FROM EvilSchemes WHERE SchemeId = @id", _connection);
    command.Parameters.AddWithValue("@id", schemeId);
    command.ExecuteNonQuery();
    // What if minions are assigned? Equipment? No checks!
}
```

**Example 3 - Generic Try/Catch (SchemeManagementForm.cs, Lines 360-388)**:
```csharp
try
{
    var newScheme = new EvilScheme
    {
        Name = txtName.Text,
        Description = txtDescription.Text,
        // ... more properties
    };

    // Direct database call from event handler (anti-pattern)
    DatabaseHelper.InsertScheme(newScheme);

    // Reload to get the ID
    var schemes = DatabaseHelper.GetAllSchemes();
    binding.Clear();
    foreach (var s in schemes)
        binding.Add(s);

    MessageBox.Show("Scheme added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
catch (Exception ex)
{
    // Catches EVERYTHING - too broad (anti-pattern)
    MessageBox.Show($"Failed to add scheme: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

### Why It's Problematic

1. **Silent failures**: Application crashes or corrupts data
2. **Poor user experience**: Generic error messages don't help users
3. **Data integrity issues**: Deletes without checking foreign keys
4. **No logging**: Errors are shown but not recorded
5. **Catching too broadly**: `catch (Exception)` hides programming errors

### Impact on Testability

- **Cannot test error scenarios**: No error paths to test
- **Unpredictable behavior**: Errors propagate unpredictably
- **Hard to verify error handling**: No structured error responses
- **Integration test failures**: Tests fail unexpectedly due to uncaught errors

### Impact on Maintainability

- **Debugging difficulty**: No context when errors occur
- **Data corruption risk**: Operations succeed partially
- **Poor diagnostics**: Cannot trace failure causes
- **Support burden**: Users cannot provide actionable error reports

### Recommended Solution

```csharp
// Structured error handling
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public Error Error { get; }
    
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(Error error) => new Result<T>(error);
}

// Service layer with validation
public class SchemeService
{
    public Result<int> DeleteScheme(int schemeId)
    {
        // Check dependencies
        var assignedMinions = _minionRepo.GetBySchemeId(schemeId);
        if (assignedMinions.Any())
            return Result<int>.Failure(new Error("Cannot delete scheme with assigned minions"));
            
        var assignedEquipment = _equipmentRepo.GetBySchemeId(schemeId);
        if (assignedEquipment.Any())
            return Result<int>.Failure(new Error("Cannot delete scheme with assigned equipment"));
            
        try
        {
            _schemeRepo.Delete(schemeId);
            _logger.LogInformation($"Deleted scheme {schemeId}");
            return Result<int>.Success(schemeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete scheme {schemeId}");
            return Result<int>.Failure(new Error("Database error occurred"));
        }
    }
}

// Program.cs with proper error handling
try
{
    DatabaseHelper.Initialize();
    DatabaseHelper.CreateSchemaIfNotExists();
    DatabaseHelper.SeedInitialData();
    Application.Run(new MainForm());
}
catch (Exception ex)
{
    MessageBox.Show($"Fatal error during initialization: {ex.Message}", "Startup Error", 
        MessageBoxButtons.OK, MessageBoxIcon.Error);
    // Log the error
    Logger.LogFatal(ex);
}
```

---

## Anti-Pattern 7: Tight Coupling to Infrastructure

### Location

**Throughout Models**:
- `Models/Minion.cs` - Lines 36 (calls `DatabaseHelper.UpdateMinion`)
- `Models/EvilScheme.cs` - Lines 32-36 (calls `DatabaseHelper.GetAllMinions`)
- `Models/Equipment.cs` - Lines 23, 34 (calls `DatabaseHelper`)
- `Models/SecretBase.cs` - Lines 22 (calls `DatabaseHelper.GetBaseOccupancy`)

### Description

Domain models and business logic directly reference infrastructure classes (DatabaseHelper, ConfigManager), creating tight coupling.

**Example from Minion.cs (Lines 24-38)**:
```csharp
public void UpdateMood()
{
    // Business rules embedded in model
    if (this.LoyaltyScore > ConfigManager.HighLoyaltyThreshold)
        this.MoodStatus = ConfigManager.MoodHappy;
    else if (this.LoyaltyScore < ConfigManager.LowLoyaltyThreshold)
        this.MoodStatus = ConfigManager.MoodBetrayal;
    else
        this.MoodStatus = ConfigManager.MoodGrumpy;

    this.LastMoodUpdate = DateTime.Now;

    // Directly accesses database (anti-pattern)
    DatabaseHelper.UpdateMinion(this);
}
```

**Example from SecretBase.cs (Lines 19-28)**:
```csharp
public int GetCurrentOccupancy()
{
    // Directly calls database (anti-pattern)
    return DatabaseHelper.GetBaseOccupancy(this.BaseId);
}

public bool CanAccommodateMinion()
{
    return GetCurrentOccupancy() < Capacity;
}
```

### Why It's Problematic

1. **Cannot unit test**: Models require full infrastructure
2. **Violates Dependency Inversion Principle**: Depends on concrete implementations
3. **Hard to mock**: Static calls cannot be intercepted
4. **Poor portability**: Cannot reuse models in different contexts
5. **Tight coupling**: Changes to infrastructure break domain models

### Impact on Testability

- **Requires full stack for tests**: Cannot test models in isolation
- **Slow tests**: Every test hits database
- **Cannot mock dependencies**: Static calls are not mockable
- **Integration tests only**: No true unit tests possible

### Impact on Maintainability

- **Difficult to change infrastructure**: Models depend on specific implementation
- **Cannot reuse logic**: Models tied to WinForms + SQLite environment
- **Poor separation of concerns**: Domain logic mixed with infrastructure
- **Refactoring difficulty**: Changes cascade across layers

### Recommended Solution

```csharp
// Pure domain model (no infrastructure dependencies)
public class Minion
{
    // Properties only
    public void UpdateMood(int highLoyaltyThreshold, int lowLoyaltyThreshold)
    {
        MoodStatus = LoyaltyScore switch
        {
            var l when l > highLoyaltyThreshold => "Happy",
            var l when l < lowLoyaltyThreshold => "Plotting Betrayal",
            _ => "Grumpy"
        };
        LastMoodUpdate = DateTime.UtcNow;
    }
}

// Application service with dependencies injected
public class MinionService
{
    private readonly IMinionRepository _repository;
    private readonly BusinessRulesConfig _config;
    
    public MinionService(IMinionRepository repository, BusinessRulesConfig config)
    {
        _repository = repository;
        _config = config;
    }
    
    public void UpdateMinionMood(int minionId)
    {
        var minion = _repository.GetById(minionId);
        minion.UpdateMood(_config.HighLoyaltyThreshold, _config.LowLoyaltyThreshold);
        _repository.Update(minion);
    }
}
```

---

## Key Business Rules Documentation

Based on the contracts and code analysis, here are the core business rules implemented in the system:

### 1. Minion Loyalty System

**Rule ID**: BR-M-001  
**Location**: `Models/Minion.cs` (Lines 51-64), `contracts/minion-rules.md`

**Description**: Minion loyalty changes monthly based on salary payment.

**Formula**:
```
IF (ActualSalaryPaid >= SalaryDemand) THEN
    LoyaltyScore = LoyaltyScore + 3
ELSE
    LoyaltyScore = LoyaltyScore - 5
END IF

LoyaltyScore = CLAMP(LoyaltyScore, 0, 100)
```

**Constants**:
- `LoyaltyGrowthRate = 3` (ConfigManager.cs, Line 16)
- `LoyaltyDecayRate = 5` (ConfigManager.cs, Line 15)

**Implementation Issues**:
- ✗ Logic in model instead of service
- ✗ Direct database call from model
- ✗ No tests for edge cases (0, 100 boundaries)

---

### 2. Minion Mood Determination

**Rule ID**: BR-M-002  
**Location**: `Models/Minion.cs` (Lines 24-38), `contracts/minion-rules.md`

**Description**: Mood is calculated from loyalty score and time on scheme.

**Formula**:
```
IF (LoyaltyScore > 70 AND DaysOnScheme <= 60) THEN
    Mood = "Happy"
ELSE IF (DaysOnScheme > 60) THEN
    Mood = "Exhausted"
ELSE IF (LoyaltyScore >= 40 AND LoyaltyScore <= 70) THEN
    Mood = "Grumpy"
ELSE IF (LoyaltyScore < 40) THEN
    Mood = "Plotting Betrayal"
END IF
```

**Constants**:
- `HighLoyaltyThreshold = 70` (ConfigManager.cs, Line 52)
- `LowLoyaltyThreshold = 40` (ConfigManager.cs, Line 51)
- `OverworkedDays = 60` (ConfigManager.cs, Line 52)

**Mood Impact on Effectiveness**:
- "Happy": 110% skill contribution
- "Grumpy": 100% skill contribution (normal)
- "Exhausted": 50% skill contribution
- "Plotting Betrayal": 20% sabotage chance

**Implementation Issues**:
- ✗ Simplified version only (doesn't check DaysOnScheme)
- ✗ No implementation of effectiveness multipliers
- ✗ No sabotage mechanics implemented

---

### 3. Scheme Success Likelihood Calculation

**Rule ID**: BR-S-001  
**Location**: `Models/EvilScheme.cs` (Lines 27-73), `Forms/SchemeManagementForm.cs` (Lines 522-603), `contracts/scheme-rules.md`

**Description**: Success probability calculated from resources, budget, and timeline.

**Formula**:
```
BaseSuccess = 50

MinionBonus = MatchingSpecialtyMinions * 10
EquipmentBonus = WorkingEquipmentCount * 5

BudgetPenalty = (CurrentSpending > Budget) ? -20 : 0
ResourcePenalty = (TotalMinions >= 2 AND MatchingMinions >= 1) ? 0 : -15
TimelinePenalty = (Today > TargetCompletionDate) ? -25 : 0

SuccessLikelihood = BaseSuccess + MinionBonus + EquipmentBonus + BudgetPenalty + ResourcePenalty + TimelinePenalty

SuccessLikelihood = CLAMP(SuccessLikelihood, 0, 100)
```

**Constants**:
- Base success = 50
- Minion bonus = 10 per matching minion
- Equipment bonus = 5 per working equipment
- Budget penalty = -20
- Resource penalty = -15
- Timeline penalty = -25
- Minimum equipment condition = 50 (ConfigManager.cs, Line 54)

**Implementation Issues**:
- ✗ **DUPLICATED** in model and form (81 lines of identical logic)
- ✗ Database calls in calculation logic
- ✗ Magic numbers (10, 5, -20, -15, -25) not configurable

---

### 4. Equipment Degradation

**Rule ID**: BR-E-001  
**Location**: `Models/Equipment.cs` (Lines 22-35), `contracts/equipment-rules.md`

**Description**: Equipment condition degrades when assigned to active schemes.

**Formula**:
```
IF (AssignedToScheme AND Scheme.Status = "Active") THEN
    MonthsSinceMaintenance = MONTHS_BETWEEN(Today, LastMaintenanceDate)
    Degradation = MonthsSinceMaintenance * 5
    
    NewCondition = Condition - Degradation
    NewCondition = MAX(0, NewCondition)
END IF
```

**Constants**:
- `ConditionDegradationRate = 5` (ConfigManager.cs, Line 17)
- `MinEquipmentCondition = 50` (for scheme assignment)
- `BrokenEquipmentCondition = 20` (visual indicator)

**Condition States**:
- >= 50: Operational (can be assigned)
- 20-49: Degraded (can work but at risk)
- < 20: Broken (shown in red, needs maintenance)
- 0: Non-functional

**Implementation Issues**:
- ✗ Simplified (always uses 1 month instead of calculating from date)
- ✗ Database call in model method
- ✗ No automatic degradation process

---

### 5. Equipment Maintenance Costs

**Rule ID**: BR-E-002  
**Location**: `Models/Equipment.cs` (Lines 37-51), `contracts/equipment-rules.md`

**Description**: Maintenance restores equipment to 100% condition at a cost.

**Formula**:
```
IF (Category = "Doomsday Device") THEN
    MaintenanceCost = PurchasePrice * 0.30
ELSE
    MaintenanceCost = PurchasePrice * 0.15
END IF

Condition = 100
LastMaintenanceDate = Today
```

**Constants**:
- `MaintenanceCostPercentage = 0.15` (15%, ConfigManager.cs, Line 18)
- `DoomsdayMaintenanceCostPercentage = 0.30` (30%, ConfigManager.cs, Line 19)

**Implementation Issues**:
- ✗ No budget checking before maintenance
- ✗ Database call in model method
- ✗ Returns cost but doesn't deduct from budget

---

### 6. Scheme Assignment Validation

**Rule ID**: BR-S-003  
**Location**: Partially in forms, `contracts/scheme-rules.md`

**Description**: Schemes require minimum resources to be viable.

**Requirements**:
```
MinimumMinions = 2
MinimumMatchingSpecialty = 1
MinimumEquipment = 0 (optional but affects success)
Budget > 0
TargetCompletionDate > Today
```

**Implementation Issues**:
- ✗ Validation scattered across forms
- ✗ Only partial checks implemented
- ✗ No enforcement at database level

---

### 7. Base Capacity Management

**Rule ID**: BR-B-001  
**Location**: `Models/SecretBase.cs` (Lines 19-32)

**Description**: Secret bases have limited capacity for minions.

**Formula**:
```
CurrentOccupancy = COUNT(Minions WHERE CurrentBaseId = BaseId)
AvailableCapacity = Capacity - CurrentOccupancy
CanAccommodateMinion = (CurrentOccupancy < Capacity)
```

**Implementation Issues**:
- ✗ No enforcement when assigning minions
- ✗ Database call in model property
- ✗ No UI indication of capacity status

---

### 8. Skill Level Matching

**Rule ID**: BR-M-003  
**Location**: Implied in scheme success calculation

**Description**: Minions should have skill level >= scheme required skill for effectiveness.

**Implementation**:
- Currently only specialty matching is checked
- Skill level is captured but not used in success calculation

**Missing Implementation**:
- ✗ No bonus/penalty for skill level matching
- ✗ Skill level not validated on assignment
- ✗ No UI indication of skill mismatch

---

## Summary of Critical Issues

### By Severity

**Critical (Blocking Production Use)**:
1. No error handling in initialization (application crashes)
2. Duplicated business logic (inconsistent calculations)
3. No data validation on deletes (data corruption)

**High (Major Maintenance/Test Issues)**:
4. God class DatabaseHelper (untestable)
5. Business logic in UI (cannot reuse)
6. Tight coupling to infrastructure (hard to change)

**Medium (Technical Debt)**:
7. Magic strings and numbers (maintenance burden)
8. Anemic domain models (poor OOP design)

### By Impact on Testability

| Anti-Pattern | Unit Test Impact | Integration Test Impact | Overall Grade |
|--------------|-----------------|------------------------|---------------|
| God Class DatabaseHelper | ⚠️ Impossible | ⚠️ Difficult | **F** |
| Business Logic in UI | ⚠️ Impossible | ⚠️ Requires UI automation | **F** |
| Duplicate Code | ⚠️ Multiple suites needed | ✓ Possible | **D** |
| Anemic Models | ⚠️ Requires infrastructure | ⚠️ Slow | **D** |
| Tight Coupling | ⚠️ Cannot isolate | ⚠️ Requires full stack | **F** |
| No Error Handling | ⚠️ Unpredictable | ⚠️ Flaky tests | **D** |
| Magic Values | ⚠️ Hard to parameterize | ✓ Possible | **C** |

### By Impact on Maintainability

| Anti-Pattern | Change Amplification | Defect Risk | Cognitive Load | Overall Grade |
|--------------|---------------------|-------------|----------------|---------------|
| God Class | ⚠️ High | ⚠️ High | ⚠️ Very High | **F** |
| Duplicate Code | ⚠️ Very High | ⚠️ Very High | ⚠️ High | **F** |
| Business Logic in UI | ⚠️ High | ⚠️ High | ⚠️ High | **F** |
| Magic Values | ⚠️ High | ⚠️ Medium | ⚠️ Medium | **D** |
| Anemic Models | ⚠️ Medium | ✓ Low | ⚠️ Medium | **C** |
| No Error Handling | ⚠️ Medium | ⚠️ Very High | ⚠️ Medium | **D** |
| Tight Coupling | ⚠️ High | ⚠️ High | ⚠️ High | **F** |

---

## Recommendations

### Immediate Actions (High Priority)

1. **Add error handling to Program.cs** - Prevent application crashes
2. **Consolidate success calculation** - Single source of truth
3. **Add validation to delete operations** - Prevent data corruption

### Short-term Refactoring (Sprint-level)

4. **Extract validation service** - Remove validation from UI
5. **Create repository interfaces** - Enable dependency injection
6. **Replace magic strings with enums** - Type safety

### Long-term Architecture (Multiple Sprints)

7. **Implement repository pattern** - Replace DatabaseHelper
8. **Create service layer** - Move business logic from models and UI
9. **Implement proper error handling strategy** - Structured errors and logging
10. **Add comprehensive unit tests** - Once dependencies are injectable

---

## Conclusion

The Villain Lair Manager codebase exhibits classic symptoms of rapid development without architectural planning. The seven identified anti-patterns create a compound effect, making the code difficult to test, maintain, and extend.

The most critical issue is the **God Class DatabaseHelper**, which acts as a bottleneck for testability. Combined with **business logic in the UI layer** and **duplicated calculations**, the application is essentially untestable without major refactoring.

A systematic refactoring approach, starting with dependency injection and repository pattern, would significantly improve code quality and enable proper automated testing.

**Overall Codebase Grade**: **D-**
- Testability: **F** (virtually untestable)
- Maintainability: **D** (high change amplification)
- Correctness: **C** (business rules mostly implemented)
- Performance: **B** (adequate for current scale)
