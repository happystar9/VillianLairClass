# Data Model: Super Villain Lair Management System

**Feature**: 001-untangle-training-app
**Last Updated**: 2025-12-01

## Entity Relationship Diagram

```
┌─────────────┐           ┌──────────────┐
│   Minions   │───────────│ EvilSchemes  │
│             │  assigned │              │
│ - MinionId  │    to     │ - SchemeId   │
│ - Name      │           │ - Name       │
│ - Specialty │           │ - Budget     │
│ - Loyalty   │           │ - Status     │
└──────┬──────┘           └──────┬───────┘
       │                         │
       │ stationed               │ assigned
       │    at                   │    to
       │                         │
┌──────┴──────┐           ┌──────┴───────┐
│SecretBases  │           │  Equipment   │
│             │  stores   │              │
│ - BaseId    │───────────│ - EquipmentId│
│ - Location  │           │ - Category   │
│ - Capacity  │           │ - Condition  │
└─────────────┘           └──────────────┘
```

## Entities

### Minion

Represents a loyal (or not-so-loyal) henchman working for the evil genius.

**Attributes**:
- `MinionId` (integer, PK, auto-increment): Unique identifier
- `Name` (text, required): Minion's name (e.g., "Igor", "Boris", "Helga")
- `SkillLevel` (integer, required): Competence level from 1-10
- `Specialty` (text, required): Area of expertise (Hacking, Explosives, Disguise, Combat)
- `LoyaltyScore` (integer, required): Loyalty from 0-100
- `SalaryDemand` (real, required): Monthly salary in evil dollars
- `CurrentBaseId` (integer, nullable, FK): Which base they're stationed at
- `CurrentSchemeId` (integer, nullable, FK): Which scheme they're assigned to
- `MoodStatus` (text, required): Current mood ("Happy", "Grumpy", "Plotting Betrayal", "Exhausted")
- `LastMoodUpdate` (text, required): ISO date string of last mood calculation

**Relationships**:
- Many-to-one with SecretBases (stationed at)
- Many-to-one with EvilSchemes (assigned to)

**Business Rules**:
- Cannot be assigned to scheme if skill level too low
- Cannot be assigned to multiple active schemes simultaneously
- Loyalty decays over time if underpaid
- Mood is calculated based on loyalty and work duration

---

### EvilScheme

Represents a diabolical plan to achieve villainous goals.

**Attributes**:
- `SchemeId` (integer, PK, auto-increment): Unique identifier
- `Name` (text, required): Scheme name (e.g., "Steal Moon", "Freeze City")
- `Description` (text, required): What the scheme entails
- `Budget` (real, required): Total allocated budget in evil dollars
- `CurrentSpending` (real, default 0): Running total of expenses
- `RequiredSkillLevel` (integer, required): Minimum minion skill needed
- `RequiredSpecialty` (text, required): Primary specialty needed for execution
- `Status` (text, required): Current status (Planning, Active, On Hold, Completed, Failed)
- `StartDate` (text, nullable): ISO date string when scheme began
- `TargetCompletionDate` (text, required): ISO date string for deadline
- `DiabolicalRating` (integer, required): How evil is this (1-10 scale)
- `SuccessLikelihood` (integer, required): Calculated probability 0-100

**Relationships**:
- One-to-many with Minions (has assigned minions)
- One-to-many with Equipment (uses equipment)

**Business Rules**:
- Cannot exceed budget
- Status transitions have prerequisites (Planning → Active requires minions)
- Success likelihood calculated from assigned resources
- High diabolical rating requires special equipment

---

### SecretBase

Represents a hidden lair or fortress for operations.

**Attributes**:
- `BaseId` (integer, PK, auto-increment): Unique identifier
- `Name` (text, required): Base name (e.g., "Volcano Fortress", "Arctic Hideout")
- `Location` (text, required): Where it's hidden
- `Capacity` (integer, required): Maximum minions it can house
- `SecurityLevel` (integer, required): Defense rating from 1-10
- `MonthlyMaintenanceCost` (real, required): Upkeep costs
- `HasDoomsdayDevice` (integer, required): Boolean (0/1) if equipped with ultimate weapon
- `IsDiscovered` (integer, required): Boolean (0/1) if heroes found it
- `LastInspectionDate` (text, nullable): ISO date string of last inspection

**Relationships**:
- One-to-many with Minions (houses minions)
- One-to-many with Equipment (stores equipment)

**Business Rules**:
- Cannot house more minions than capacity
- Discovered bases should be evacuated
- Maintenance costs scale with capacity and security level

---

### Equipment

Represents tools, weapons, vehicles, and doomsday devices.

**Attributes**:
- `EquipmentId` (integer, PK, auto-increment): Unique identifier
- `Name` (text, required): Equipment name (e.g., "Freeze Ray", "Drill Tank")
- `Category` (text, required): Type (Weapon, Vehicle, Gadget, Doomsday Device)
- `Condition` (integer, required): Working condition 0-100 percentage
- `PurchasePrice` (real, required): Initial acquisition cost
- `MaintenanceCost` (real, required): Monthly upkeep cost
- `AssignedToSchemeId` (integer, nullable, FK): Which scheme is using it
- `StoredAtBaseId` (integer, nullable, FK): Where it's kept
- `RequiresSpecialist` (integer, required): Boolean (0/1) if needs skilled operator
- `LastMaintenanceDate` (text, nullable): ISO date string of last service

**Relationships**:
- Many-to-one with EvilSchemes (assigned to)
- Many-to-one with SecretBases (stored at)

**Business Rules**:
- Condition degrades when in active use
- Cannot be assigned if condition too low
- Specialist equipment requires skilled minion on scheme
- Must be stored before assignment

## Database Schema (SQLite)

### CREATE TABLE Statements

```sql
-- Minions table
CREATE TABLE Minions (
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

-- Evil Schemes table
CREATE TABLE EvilSchemes (
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

-- Secret Bases table
CREATE TABLE SecretBases (
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

-- Equipment table
CREATE TABLE Equipment (
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
```

### Indexes (Performance Optimization)

```sql
-- Foreign key indexes for joins
CREATE INDEX idx_minions_base ON Minions(CurrentBaseId);
CREATE INDEX idx_minions_scheme ON Minions(CurrentSchemeId);
CREATE INDEX idx_equipment_scheme ON Equipment(AssignedToSchemeId);
CREATE INDEX idx_equipment_base ON Equipment(StoredAtBaseId);

-- Status filtering
CREATE INDEX idx_schemes_status ON EvilSchemes(Status);
CREATE INDEX idx_equipment_category ON Equipment(Category);
```

## Sample Data

### Sample Minions

| MinionId | Name | SkillLevel | Specialty | LoyaltyScore | SalaryDemand | MoodStatus |
|----------|------|------------|-----------|--------------|--------------|------------|
| 1 | Igor | 3 | Combat | 85 | 3000 | Happy |
| 2 | Helga | 8 | Hacking | 45 | 8000 | Grumpy |
| 3 | Boris | 6 | Explosives | 92 | 5500 | Happy |
| 4 | Natasha | 9 | Disguise | 25 | 9500 | Plotting Betrayal |

### Sample Evil Schemes

| SchemeId | Name | Budget | RequiredSkillLevel | DiabolicalRating | Status |
|----------|------|--------|-------------------|------------------|--------|
| 1 | Steal the Moon | 1000000 | 8 | 10 | Planning |
| 2 | Freeze Entire City | 500000 | 6 | 8 | Active |
| 3 | Replace World Leaders | 750000 | 9 | 9 | Planning |

### Sample Secret Bases

| BaseId | Name | Location | Capacity | SecurityLevel | HasDoomsdayDevice |
|--------|------|----------|----------|---------------|-------------------|
| 1 | Volcano Fortress | Pacific Island | 50 | 9 | 1 |
| 2 | Arctic Hideout | North Pole | 30 | 7 | 0 |
| 3 | Underwater Lair | Mariana Trench | 40 | 10 | 1 |

### Sample Equipment

| EquipmentId | Name | Category | Condition | RequiresSpecialist |
|-------------|------|----------|-----------|-------------------|
| 1 | Freeze Ray | Weapon | 85 | 1 |
| 2 | Drill Tank | Vehicle | 72 | 0 |
| 3 | Shrink Ray | Doomsday Device | 95 | 1 |
| 4 | Invisibility Cloak | Gadget | 60 | 0 |

## Relationship Queries

### Common Join Patterns

**Get all minions with their base location**:
```sql
SELECT m.Name, m.Specialty, b.Name as BaseName, b.Location
FROM Minions m
LEFT JOIN SecretBases b ON m.CurrentBaseId = b.BaseId;
```

**Get scheme with assigned minions count**:
```sql
SELECT s.Name, s.Status, COUNT(m.MinionId) as MinionCount
FROM EvilSchemes s
LEFT JOIN Minions m ON m.CurrentSchemeId = s.SchemeId
GROUP BY s.SchemeId;
```

**Get base occupancy**:
```sql
SELECT b.Name, b.Capacity, COUNT(m.MinionId) as CurrentOccupancy,
       (b.Capacity - COUNT(m.MinionId)) as AvailableSpace
FROM SecretBases b
LEFT JOIN Minions m ON m.CurrentBaseId = b.BaseId
GROUP BY b.BaseId;
```

**Get total monthly costs**:
```sql
SELECT
    (SELECT SUM(SalaryDemand) FROM Minions) +
    (SELECT SUM(MonthlyMaintenanceCost) FROM SecretBases) +
    (SELECT SUM(MaintenanceCost) FROM Equipment)
as TotalMonthlyCost;
```

## Migration and Seed Strategy

**Initial Setup**:
1. Application checks if database file exists
2. If not, creates file and runs CREATE TABLE statements
3. Seeds initial data (5-10 records per table)
4. Sets up indexes

**Seed Data Characteristics**:
- Mix of happy and disgruntled minions
- Some schemes in progress, some in planning
- Various base capacities and locations
- Equipment in different condition states
- Realistic relationships (minions at bases, equipment stored, etc.)

This provides students with interesting data to explore and test with immediately.
