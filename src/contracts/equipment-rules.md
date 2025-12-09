# Business Rules: Equipment Management

**Feature**: 001-untangle-training-app
**Entity**: Equipment
**Last Updated**: 2025-12-01

## Overview

Equipment represents the tools, weapons, vehicles, and doomsday devices used in evil schemes. Proper maintenance and assignment are crucial for scheme success. This document specifies the business rules for equipment management.

## Rule 1: Condition Degradation
## Test
**Rule ID**: BR-E-001
**Priority**: High
**Category**: Behavioral

### Description

Equipment condition degrades over time when in active use on schemes.

### Specification

```
IF (Equipment.AssignedToSchemeId IS NOT NULL AND
    Scheme.Status = "Active") THEN

    MonthsSinceLastMaintenance = MONTHS_BETWEEN(Today, LastMaintenanceDate)
    DegradationRate = 5  // 5% per month in active use

    NewCondition = Condition - (MonthsSinceLastMaintenance * DegradationRate)
    NewCondition = MAX(0, NewCondition)  // Cannot go below 0

    Equipment.Condition = NewCondition
ELSE
    // Not in active use - no degradation
    Equipment.Condition = Equipment.Condition
END IF
```

### Test Cases

| Initial Condition | Months Since Maintenance | In Active Use? | Expected Condition | Notes |
|------------------|------------------------|----------------|-------------------|-------|
| 100 | 1 | Yes | 95 | Standard degradation |
| 100 | 5 | Yes | 75 | Extended use |
| 30 | 8 | Yes | 0 | Severely degraded (clamped) |
| 80 | 3 | No | 80 | No degradation when not assigned |
| 50 | 2 | Yes (Completed scheme) | 50 | No degradation for completed schemes |

### Edge Cases

- If LastMaintenanceDate is NULL, use equipment purchase date or creation date
- Condition cannot drop below 0
- Degradation is calculated when:
  - Form loads equipment list
  - Before equipment assignment
  - When performing maintenance
- Equipment in "Planning" or "On Hold" schemes degrades at 50% rate (2.5% per month)

### Business Impact

- Condition < 50%: Equipment cannot be assigned to new schemes
- Condition < 20%: Equipment shown as "Broken" in UI (red text)
- Condition = 0: Equipment is non-functional and must be repaired before any use

---

## Rule 2: Maintenance Operations

**Rule ID**: BR-E-002
**Priority**: High
**Category**: Business Logic

### Description

Equipment can be maintained to restore condition to 100%, but this costs money.

### Specification

```
MaintenanceCost = Equipment.PurchasePrice * 0.15  // 15% of original price

IF (Equipment.Condition >= 100) THEN
    Maintenance = REJECTED("Equipment is already in perfect condition")
ELSE IF (AvailableFunds < MaintenanceCost) THEN
    Maintenance = REJECTED("Insufficient funds for maintenance")
    // Note: AvailableFunds check is often missing (anti-pattern)
ELSE
    Equipment.Condition = 100
    Equipment.LastMaintenanceDate = Today
    DeductFromBudget(MaintenanceCost)
    Maintenance = SUCCESS
END IF
```

### Test Cases

| Condition | Purchase Price | Maintenance Cost | Available Funds | Result | New Condition |
|-----------|---------------|-----------------|----------------|--------|---------------|
| 50 | 10000 | 1500 | 5000 | Success | 100 |
| 80 | 20000 | 3000 | 2000 | Rejected | 80 (unchanged) |
| 100 | 10000 | 1500 | 5000 | Rejected | 100 (already perfect) |
| 15 | 50000 | 7500 | 10000 | Success | 100 |

### Implementation Note (Anti-Pattern)

The funds check is often missing or inconsistent:
- Sometimes checks against total scheme budgets
- Sometimes allows negative balance
- Sometimes doesn't check at all (just performs maintenance)

Students should identify this as a bug and add proper budget tracking.

---

## Rule 3: Assignment Validation

**Rule ID**: BR-E-003
**Priority**: Critical
**Category**: Validation

### Description

Equipment can only be assigned to schemes if it meets condition and location requirements.

### Specification

```
// Validation checks (ALL must pass)

Check 1: Condition Requirement
IF (Equipment.Condition < 50) THEN
    Assignment = REJECTED("Equipment condition too low for use")
END IF

Check 2: Storage Location
IF (Equipment.StoredAtBaseId IS NULL) THEN
    Assignment = REJECTED("Equipment must be stored at a base first")
END IF

Check 3: Not Already Assigned
IF (Equipment.AssignedToSchemeId IS NOT NULL AND
    AssignedScheme.Status = "Active") THEN
    Assignment = REJECTED("Equipment already assigned to another active scheme")
END IF

Check 4: Specialist Requirement
IF (Equipment.RequiresSpecialist = 1) THEN
    SchemeMinions = LIST(Minions WHERE CurrentSchemeId = Scheme.SchemeId)
    HasSpecialist = EXISTS(SchemeMinions WHERE SkillLevel >= 8)

    IF (NOT HasSpecialist) THEN
        Assignment = REJECTED("Equipment requires a specialist minion (skill 8+)")
    END IF
END IF

Check 5: Location Match (often missing - anti-pattern)
IF (Equipment.StoredAtBaseId != Scheme.PrimaryBaseId) THEN
    // Should reject but often doesn't check
    Assignment = WARNING("Equipment is stored at different base than scheme")
END IF
```

### Test Cases

| Condition | Stored At Base? | Already Assigned? | Requires Specialist? | Has Specialist? | Result |
|-----------|----------------|------------------|---------------------|----------------|--------|
| 80 | Yes | No | No | N/A | Valid |
| 40 | Yes | No | No | N/A | Rejected (condition) |
| 80 | No | No | No | N/A | Rejected (not stored) |
| 80 | Yes | Yes (Active) | No | N/A | Rejected (already assigned) |
| 80 | Yes | No | Yes | No | Rejected (no specialist) |
| 80 | Yes | No | Yes | Yes (skill 9) | Valid |
| 80 | Yes | Yes (Completed) | No | N/A | Valid (can reassign) |

### Edge Cases

- Equipment at condition exactly 50 is valid (boundary case)
- Multiple equipment items can be assigned to same scheme
- Equipment stored at Base A can be assigned to scheme at Base B (location check missing - anti-pattern)
- Specialist equipment needs minion with skill >= 8 (not just any minion)

---

## Rule 4: Category Validation

**Rule ID**: BR-E-004
**Priority**: Medium
**Category**: Validation

### Description

Equipment category must be one of the predefined valid values.

### Specification

**Valid Categories** (case-sensitive):
- "Weapon"
- "Vehicle"
- "Gadget"
- "Doomsday Device"

**Any other value is REJECTED.**

### Test Cases

| Input Category | Result | Error Message |
|---------------|--------|---------------|
| "Weapon" | VALID | - |
| "Doomsday Device" | VALID | - |
| "weapon" | INVALID | "Invalid category (case-sensitive)" |
| "Tool" | INVALID | "Invalid category. Must be: Weapon, Vehicle, Gadget, or Doomsday Device" |
| "" | INVALID | "Category cannot be empty" |
| NULL | INVALID | "Category is required" |

### Implementation Note

Like minion specialties, this validation is duplicated across:
- Form input validation
- Model validation (sometimes)
- Database schema (no constraint - intentional)
- Hardcoded ComboBox items

Students should consolidate into a single validation location.

---

## Rule 5: Condition Range Validation

**Rule ID**: BR-E-005
**Priority**: Medium
**Category**: Validation

### Description

Equipment condition must be between 0 and 100 inclusive.

### Specification

```
IF (Condition < 0 OR Condition > 100) THEN
    Validation = FAILED("Condition must be between 0 and 100")
ELSE
    Validation = PASSED
END IF
```

### Test Cases

| Condition | Result | Error Message |
|-----------|--------|---------------|
| 0 | VALID | - (broken but valid) |
| 100 | VALID | - (perfect condition) |
| 50 | VALID | - |
| -10 | INVALID | "Condition must be between 0 and 100" |
| 150 | INVALID | "Condition must be between 0 and 100" |

### UI Guidance

- Use NumericUpDown control with min=0, max=100
- Color-code condition in data grid:
  - Green: 70-100
  - Yellow: 50-69
  - Orange: 20-49
  - Red: 0-19

---

## Rule 6: Cost Validation

**Rule ID**: BR-E-006
**Priority**: Low
**Category**: Validation

### Description

Equipment purchase price and maintenance cost must be positive numbers.

### Specification

```
IF (PurchasePrice <= 0) THEN
    Validation = FAILED("Purchase price must be greater than zero")
ELSE IF (MaintenanceCost < 0) THEN
    Validation = FAILED("Maintenance cost cannot be negative")
ELSE IF (MaintenanceCost > PurchasePrice) THEN
    Validation = WARNING("Maintenance cost exceeds purchase price - unusual")
    // But still allow
ELSE
    Validation = PASSED
END IF
```

### Test Cases

| Purchase Price | Maintenance Cost | Result | Message |
|---------------|-----------------|--------|---------|
| 10000 | 500 | VALID | - |
| 0 | 500 | INVALID | "Purchase price must be greater than zero" |
| 10000 | -100 | INVALID | "Maintenance cost cannot be negative" |
| 10000 | 15000 | VALID (warning) | "Maintenance cost exceeds purchase price" |

### Business Context

Typical equipment costs:
- **Weapons**: 5,000 - 50,000 (maintenance: 10-20% of price)
- **Vehicles**: 20,000 - 100,000 (maintenance: 15-25% of price)
- **Gadgets**: 2,000 - 30,000 (maintenance: 5-15% of price)
- **Doomsday Devices**: 100,000 - 1,000,000 (maintenance: 20-30% of price)

---

## Rule 7: Doomsday Device Special Rules

**Rule ID**: BR-E-007
**Priority**: Medium
**Category**: Business Logic

### Description

Doomsday devices have special handling requirements and restrictions.

### Specification

```
IF (Equipment.Category = "Doomsday Device") THEN
    // Special requirements
    RequiresSpecialist = 1  // Always requires specialist
    MinimumSkillLevel = 9   // Requires highly skilled minion

    // Storage requirements
    IF (StoredAtBase.HasDoomsdayDevice = 0) THEN
        Storage = WARNING("Base not equipped to store doomsday devices")
        // But allow anyway (anti-pattern - should enforce)
    END IF

    // Assignment requirements
    IF (Scheme.DiabolicalRating < 8) THEN
        Assignment = WARNING("Doomsday device overkill for low-rated scheme")
        // But allow anyway
    END IF

    // Maintenance cost
    MaintenanceCost = PurchasePrice * 0.30  // 30% (higher than normal)
END IF
```

### Test Cases

| Category | Base Has DD Storage? | Scheme Rating | Assigned Minion Skill | Result |
|----------|---------------------|---------------|---------------------|--------|
| Doomsday Device | Yes | 9 | 10 | Valid |
| Doomsday Device | No | 9 | 10 | Valid (with warning) |
| Doomsday Device | Yes | 5 | 10 | Valid (with warning) |
| Doomsday Device | Yes | 9 | 7 | Rejected (skill too low) |

### Edge Cases

- Multiple doomsday devices can exist in database
- Only bases with HasDoomsdayDevice=1 should store them (but not enforced)
- Schemes with rating 8+ should prefer doomsday devices
- Maintenance of doomsday devices is expensive (30% vs normal 15%)

---

## Integration Rules

### Cross-Entity Impacts

**When equipment is assigned to scheme**:
1. Update AssignedToSchemeId
2. Recalculate scheme success likelihood (+5% per equipment)
3. Start condition degradation tracking
4. Validate specialist requirement (if applicable)

**When equipment is unassigned**:
1. Set AssignedToSchemeId = NULL
2. Recalculate scheme success likelihood (-5%)
3. Stop condition degradation
4. Return to storage at base

**When equipment is deleted**:
1. Unassign from scheme (if assigned)
2. Recalculate affected scheme success likelihood
3. Remove from base storage

**When equipment condition drops below 50%**:
1. Auto-unassign from active schemes (often missing - anti-pattern)
2. Send alert to user
3. Mark as "needs maintenance" in UI

**When equipment is maintained**:
1. Restore condition to 100%
2. Update LastMaintenanceDate
3. Deduct cost from available budget
4. If was unassigned due to low condition, can be reassigned

---

## Testing Recommendations for Students

### Unit Tests to Write

1. **Condition degradation** (Rule BR-E-001):
   - Test degradation for active use
   - Test no degradation when not assigned
   - Test boundary conditions (cannot go below 0)
   - Test degradation rate calculation

2. **Maintenance operations** (Rule BR-E-002):
   - Test cost calculation
   - Test condition restoration
   - Test rejection when already perfect
   - Test budget checking (if implemented)

3. **Assignment validation** (Rule BR-E-003):
   - Test each rejection condition independently
   - Test specialist requirement
   - Test condition threshold
   - Test storage requirement

### Integration Tests to Write

1. **Category validation** (Rule BR-E-004):
   - Test all valid categories
   - Test invalid inputs
   - Test case sensitivity

2. **Doomsday device rules** (Rule BR-E-007):
   - Test specialist requirement enforcement
   - Test storage requirements
   - Test higher maintenance costs
   - Test assignment to high-rated schemes

### Refactoring Opportunities

Students should identify and fix:
- ✗ Condition degradation calculated inconsistently (sometimes on load, sometimes never)
- ✗ Maintenance cost calculation duplicated in Form and Model
- ✗ Assignment validation checks scattered across multiple forms
- ✗ Category validation hardcoded in 4+ places
- ✗ Budget checking missing from maintenance operation
- ✗ No automatic unassignment when condition drops below threshold
- ✗ Specialist requirement check duplicated from scheme assignment logic

---

## Complex Scenario Examples

### Scenario 1: Equipment Lifecycle

1. Purchase Freeze Ray: Condition = 100, Category = "Weapon"
2. Store at Volcano Fortress: StoredAtBaseId = 1
3. Assign to "Freeze City" scheme: AssignedToSchemeId = 2
4. After 3 months active: Condition = 85 (degraded 15%)
5. After 10 months active: Condition = 50 (degraded 50%)
6. Cannot assign to new scheme: Condition < 50 threshold
7. Perform maintenance: Condition = 100, cost = 15% of purchase price
8. Can now be assigned to new schemes again

### Scenario 2: Doomsday Device Management

1. Purchase Shrink Ray: Category = "Doomsday Device", Price = 500,000
2. Requires storage at base with HasDoomsdayDevice = 1
3. Assign to scheme with DiabolicalRating = 9
4. Requires minion with SkillLevel >= 9 on the scheme
5. Adds significant success likelihood bonus (+5%)
6. Maintenance cost = 150,000 (30% of price)
7. Degradation rate same as other equipment (5% per month)

### Scenario 3: Validation Chain

1. Attempt to assign "Drill Tank" to scheme
2. Check 1: Condition = 45% → REJECTED (too low)
3. Perform maintenance → Condition = 100%
4. Check 2: StoredAtBaseId = NULL → REJECTED (not stored)
5. Store at Arctic Hideout → StoredAtBaseId = 2
6. Check 3: RequiresSpecialist = 1 → Check for specialist
7. Check 4: No minion with skill 8+ on scheme → REJECTED
8. Assign high-skill minion to scheme
9. All checks pass → Assignment SUCCESS

---

**Note for Instructors**: Equipment management has the most scattered validation logic. The condition degradation calculation is implemented in at least 3 places with slight variations. This makes it an excellent target for refactoring exercises.
