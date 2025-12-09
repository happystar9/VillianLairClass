# Business Rules: Minion Management

**Feature**: 001-untangle-training-app
**Entity**: Minions
**Last Updated**: 2025-12-01

## Overview

Minions are the workforce of your evil empire. Their effectiveness depends on skill, loyalty, and proper assignment to schemes and bases. This document specifies the business rules governing minion behavior and management.

## Rule 1: Loyalty Decay and Growth

**Rule ID**: BR-M-001
**Priority**: High
**Category**: Behavioral

### Description

Minion loyalty changes over time based on compensation relative to their salary demands.

### Specification

**When loyalty is calculated** (monthly or when viewing minion details):

```
IF (ActualSalaryPaid >= SalaryDemand) THEN
    LoyaltyScore = LoyaltyScore + 3
ELSE
    LoyaltyScore = LoyaltyScore - 5
END IF

// Clamp to valid range
LoyaltyScore = MAX(0, MIN(100, LoyaltyScore))
```

### Test Cases

| Scenario | Initial Loyalty | Salary Demand | Actual Paid | Expected Result | Notes |
|----------|----------------|---------------|-------------|-----------------|-------|
| Satisfied minion | 70 | 5000 | 5000 | 73 | Loyalty increases |
| Overpaid minion | 70 | 5000 | 6000 | 73 | Same increase as satisfied |
| Underpaid minion | 70 | 5000 | 4000 | 65 | Loyalty decreases |
| Minimum boundary | 3 | 3000 | 2000 | 0 | Cannot go below 0 |
| Maximum boundary | 98 | 3000 | 4000 | 100 | Cannot exceed 100 |

### Edge Cases

- If minion has no salary demand (0), treat as satisfied
- If minion is unassigned to any scheme, use 50% of salary demand as "actual paid"
- First month minions start with their initial loyalty (no decay/growth yet)

---

## Rule 2: Mood Determination

**Rule ID**: BR-M-002
**Priority**: High
**Category**: Behavioral

### Description

Minion mood is calculated based on loyalty score and time assigned to current scheme.

### Specification

```
DaysOnCurrentScheme = (Today - SchemeAssignmentDate).Days

IF (LoyaltyScore > 70 AND DaysOnCurrentScheme <= 60) THEN
    MoodStatus = "Happy"
ELSE IF (DaysOnCurrentScheme > 60) THEN
    MoodStatus = "Exhausted"
ELSE IF (LoyaltyScore >= 40 AND LoyaltyScore <= 70) THEN
    MoodStatus = "Grumpy"
ELSE IF (LoyaltyScore < 40) THEN
    MoodStatus = "Plotting Betrayal"
END IF
```

### Test Cases

| Loyalty | Days On Scheme | Expected Mood | Reason |
|---------|---------------|---------------|--------|
| 85 | 30 | Happy | High loyalty, not overworked |
| 85 | 70 | Exhausted | Overworked regardless of loyalty |
| 55 | 20 | Grumpy | Medium loyalty |
| 25 | 15 | Plotting Betrayal | Low loyalty |
| 45 | 0 | Grumpy | No scheme assignment |

### Business Impact

- "Plotting Betrayal" minions have 20% chance of sabotaging schemes
- "Exhausted" minions contribute 50% of normal skill level
- "Grumpy" minions have no impact (normal effectiveness)
- "Happy" minions contribute 110% of normal skill level

---

## Rule 3: Scheme Assignment Validation

**Rule ID**: BR-M-003
**Priority**: Critical
**Category**: Validation

### Description

Minions can only be assigned to schemes if they meet qualification requirements and availability constraints.

### Specification

**Assignment is VALID if ALL conditions are true**:

1. `Minion.SkillLevel >= Scheme.RequiredSkillLevel`
2. `Minion.Specialty == Scheme.RequiredSpecialty`
3. `Minion.CurrentSchemeId IS NULL OR Scheme.Status != "Active"`
4. `Minion exists in database`
5. `Scheme exists in database`

**Otherwise, assignment is REJECTED with appropriate error message.**

### Test Cases

| Minion Skill | Minion Specialty | Scheme Req Skill | Scheme Req Specialty | Current Scheme | Result | Error Message |
|--------------|------------------|------------------|---------------------|----------------|--------|---------------|
| 8 | Hacking | 6 | Hacking | NULL | VALID | - |
| 5 | Hacking | 6 | Hacking | NULL | INVALID | "Minion skill level too low" |
| 8 | Combat | 6 | Hacking | NULL | INVALID | "Minion specialty doesn't match" |
| 8 | Hacking | 6 | Hacking | 5 (Active) | INVALID | "Minion already assigned to active scheme" |
| 8 | Hacking | 6 | Hacking | 3 (Completed) | VALID | Can reassign from completed scheme |

### Edge Cases

- If scheme status is "Planning" or "On Hold", multiple minions can be assigned
- If minion is being reassigned from one active scheme to another, must first unassign
- Assignment increases `Scheme.CurrentSpending` by `(Minion.SalaryDemand * EstimatedMonthsRemaining)`

---

## Rule 4: Base Assignment Capacity

**Rule ID**: BR-M-004
**Priority**: High
**Category**: Validation

### Description

Secret bases have limited capacity. Cannot assign more minions than capacity allows.

### Specification

```
CurrentOccupancy = COUNT(Minions WHERE CurrentBaseId = Base.BaseId)

IF (CurrentOccupancy >= Base.Capacity) THEN
    Assignment = REJECTED("Base is at full capacity")
ELSE
    Assignment = ALLOWED
END IF
```

### Test Cases

| Base Capacity | Current Occupancy | New Assignment | Result |
|---------------|-------------------|----------------|--------|
| 50 | 45 | 1 minion | ALLOWED |
| 50 | 49 | 1 minion | ALLOWED (exactly at capacity) |
| 50 | 50 | 1 minion | REJECTED |
| 30 | 28 | 3 minions (bulk) | REJECTED (would exceed) |

### Edge Cases

- Bulk assignment (multiple minions at once) must check total capacity
- Unassigning minions from a base frees up capacity immediately
- If base capacity is reduced below current occupancy, existing assignments remain (warn user)
- "Evacuate Base" operation removes all minions and resets occupancy to 0

---

## Rule 5: Specialty Validation

**Rule ID**: BR-M-005
**Priority**: Medium
**Category**: Validation

### Description

Minion specialty must be one of the predefined valid values.

### Specification

**Valid Specialties** (case-sensitive):
- "Hacking"
- "Explosives"
- "Disguise"
- "Combat"
- "Engineering"
- "Piloting"

**Any other value is REJECTED.**

### Test Cases

| Input Specialty | Result | Error Message |
|----------------|--------|---------------|
| "Hacking" | VALID | - |
| "Combat" | VALID | - |
| "hacking" | INVALID | "Invalid specialty (case-sensitive)" |
| "Magic" | INVALID | "Invalid specialty. Must be one of: Hacking, Explosives, Disguise, Combat, Engineering, Piloting" |
| "" | INVALID | "Specialty cannot be empty" |
| NULL | INVALID | "Specialty is required" |

### Implementation Note

This validation should occur in multiple places (by design - anti-pattern):
- Form input validation (before save)
- Model property setter (in some places)
- Database constraint (NOT implemented - intentional)

Students should identify the duplication as a code smell.

---

## Rule 6: Skill Level Range Validation

**Rule ID**: BR-M-006
**Priority**: Medium
**Category**: Validation

### Description

Minion skill level must be between 1 and 10 inclusive.

### Specification

```
IF (SkillLevel < 1 OR SkillLevel > 10) THEN
    Validation = FAILED("Skill level must be between 1 and 10")
ELSE
    Validation = PASSED
END IF
```

### Test Cases

| Skill Level | Result | Error Message |
|-------------|--------|---------------|
| 1 | VALID | - |
| 10 | VALID | - |
| 5 | VALID | - |
| 0 | INVALID | "Skill level must be between 1 and 10" |
| 11 | INVALID | "Skill level must be between 1 and 10" |
| -5 | INVALID | "Skill level must be between 1 and 10" |

### Edge Cases

- Database has CHECK constraint (may or may not be enforced in SQLite)
- UI should use NumericUpDown with min=1, max=10
- Validation should occur before database save

---

## Rule 7: Salary Demand Validation

**Rule ID**: BR-M-007
**Priority**: Low
**Category**: Validation

### Description

Minion salary demand must be a positive number.

### Specification

```
IF (SalaryDemand <= 0) THEN
    Validation = FAILED("Salary must be greater than zero")
ELSE IF (SalaryDemand > 1000000) THEN
    Validation = WARNING("Salary seems unusually high - are you sure?")
    // But still allow it
ELSE
    Validation = PASSED
END IF
```

### Test Cases

| Salary Demand | Result | Message |
|---------------|--------|---------|
| 5000 | VALID | - |
| 0 | INVALID | "Salary must be greater than zero" |
| -1000 | INVALID | "Salary must be greater than zero" |
| 1500000 | VALID (with warning) | "Salary seems unusually high" |

### Business Context

Typical minion salary ranges:
- Low skill (1-3): 2,000 - 4,000 evil dollars
- Medium skill (4-7): 4,000 - 7,000 evil dollars
- High skill (8-10): 7,000 - 12,000 evil dollars

---

## Integration Rules

### Cross-Entity Impacts

**When a minion is deleted**:
1. Remove from current scheme (decrement scheme's minion count)
2. Remove from current base (free up capacity)
3. Recalculate scheme success likelihood if was assigned

**When a minion is assigned to a new scheme**:
1. Update `CurrentSchemeId`
2. Increment scheme's spending by salary cost
3. Recalculate scheme success likelihood
4. Reset "days on scheme" counter (affects mood)

**When a minion is assigned to a new base**:
1. Update `CurrentBaseId`
2. Validate base capacity (Rule BR-M-004)
3. No impact on schemes

---

## Testing Recommendations for Students

### Unit Tests to Write

1. **Loyalty calculation** (Rule BR-M-001):
   - Test satisfied minion → loyalty increases
   - Test underpaid minion → loyalty decreases
   - Test boundary conditions (0 and 100)

2. **Mood determination** (Rule BR-M-002):
   - Test each mood state trigger
   - Test overworked condition takes precedence
   - Test unassigned minions

3. **Assignment validation** (Rule BR-M-003):
   - Test each rejection condition independently
   - Test valid assignment path
   - Test edge cases (completed schemes, status transitions)

### Integration Tests to Write

1. **Base capacity enforcement** (Rule BR-M-004):
   - Test assignment that fills base exactly to capacity
   - Test assignment that exceeds capacity
   - Test bulk assignment

2. **Specialty validation** (Rule BR-M-005):
   - Test all valid specialties
   - Test invalid inputs
   - Test case sensitivity

### Refactoring Opportunities

Students should identify and fix:
- ✗ Loyalty calculation duplicated in Form and Model
- ✗ Mood determination logic in UI event handler
- ✗ Assignment validation scattered across 3 forms
- ✗ Hardcoded specialty list in 4 different places
- ✗ Static DatabaseHelper preventing test isolation

---

**Note for Instructors**: These rules are intentionally implemented inconsistently across the codebase. Some validations happen in forms, some in models, some are missing entirely. This teaches students to identify and consolidate business logic.
