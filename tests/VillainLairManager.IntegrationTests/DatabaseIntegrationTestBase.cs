using System;
using System.Data.SQLite;
using System.IO;
using VillainLairManager.Repositories;

namespace VillainLairManager.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides database setup and teardown
/// </summary>
public abstract class DatabaseIntegrationTestBase : IDisposable
{
    protected DatabaseContext Context { get; private set; }
    protected string TestDbPath { get; private set; }

    protected DatabaseIntegrationTestBase()
    {
        // Create a unique test database for each test run
        TestDbPath = Path.Combine(Path.GetTempPath(), $"test_villainlair_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={TestDbPath};Version=3;";
        Context = new DatabaseContext(connectionString);
        InitializeSchema();
    }

    private void InitializeSchema()
    {
        // Minions table
        ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Minions (
                    MinionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SkillLevel INTEGER NOT NULL CHECK(SkillLevel >= 1 AND SkillLevel <= 10),
                    Specialty TEXT NOT NULL,
                    LoyaltyScore INTEGER NOT NULL CHECK(LoyaltyScore >= 0 AND LoyaltyScore <= 100),
                    SalaryDemand REAL NOT NULL CHECK(SalaryDemand >= 0),
                    CurrentBaseId INTEGER,
                    CurrentSchemeId INTEGER,
                    MoodStatus TEXT NOT NULL,
                    LastMoodUpdate TEXT NOT NULL,
                    FOREIGN KEY (CurrentBaseId) REFERENCES SecretBases(BaseId) ON DELETE SET NULL,
                    FOREIGN KEY (CurrentSchemeId) REFERENCES EvilSchemes(SchemeId) ON DELETE SET NULL
                );
            ");

        // Evil Schemes table
        ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS EvilSchemes (
                    SchemeId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Budget REAL NOT NULL CHECK(Budget >= 0),
                    CurrentSpending REAL DEFAULT 0 CHECK(CurrentSpending >= 0),
                    RequiredSkillLevel INTEGER NOT NULL CHECK(RequiredSkillLevel >= 1 AND RequiredSkillLevel <= 10),
                    RequiredSpecialty TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    StartDate TEXT,
                    TargetCompletionDate TEXT NOT NULL,
                    DiabolicalRating INTEGER NOT NULL CHECK(DiabolicalRating >= 1 AND DiabolicalRating <= 10),
                    SuccessLikelihood INTEGER NOT NULL CHECK(SuccessLikelihood >= 0 AND SuccessLikelihood <= 100)
                );
            ");

        // Equipment table
        ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Equipment (
                    EquipmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Condition INTEGER NOT NULL CHECK(Condition >= 0 AND Condition <= 100),
                    PurchasePrice REAL NOT NULL CHECK(PurchasePrice >= 0),
                    MaintenanceCost REAL NOT NULL CHECK(MaintenanceCost >= 0),
                    AssignedToSchemeId INTEGER,
                    StoredAtBaseId INTEGER,
                    RequiresSpecialist INTEGER NOT NULL CHECK(RequiresSpecialist IN (0, 1)),
                    LastMaintenanceDate TEXT,
                    FOREIGN KEY (AssignedToSchemeId) REFERENCES EvilSchemes(SchemeId) ON DELETE SET NULL,
                    FOREIGN KEY (StoredAtBaseId) REFERENCES SecretBases(BaseId) ON DELETE SET NULL
                );
            ");

        // SecretBases table
        ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS SecretBases (
                    BaseId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    Capacity INTEGER NOT NULL CHECK(Capacity > 0),
                    SecurityLevel INTEGER NOT NULL CHECK(SecurityLevel >= 1 AND SecurityLevel <= 10),
                    MonthlyMaintenanceCost REAL NOT NULL CHECK(MonthlyMaintenanceCost >= 0),
                    HasDoomsdayDevice INTEGER NOT NULL CHECK(HasDoomsdayDevice IN (0, 1)),
                    IsDiscovered INTEGER NOT NULL CHECK(IsDiscovered IN (0, 1)),
                    LastInspectionDate TEXT
                );
            ");
    }

    private void ExecuteNonQuery(string sql)
    {
        using (var command = new SQLiteCommand(sql, Context.Connection))
        {
            command.ExecuteNonQuery();
        }
    }

    public virtual void Dispose()
    {
        // Clean up test database
        Context?.Dispose();
        if (File.Exists(TestDbPath))
        {
            File.Delete(TestDbPath);
        }
    }
}