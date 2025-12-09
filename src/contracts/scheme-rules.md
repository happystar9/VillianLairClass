# Business Rules: Evil Scheme Management

**Feature**: 001-untangle-training-app
**Entity**: EvilSchemes
**Last Updated**: 2025-12-01

## Overview

Evil schemes are the core of your villainous operations. Success depends on proper planning, adequate resources, and staying within budget. This document specifies the business rules for scheme management and execution.

## Rule 1: Success Likelihood Calculation

**Rule ID**: BR-S-001
**Priority**: Critical
**Category**: Calculation

### Description

Scheme success likelihood is dynamically calculated based on assigned resources, budget status, and timeline adherence.

### Specification

```
BaseSuccessLikelihood = 50

// Bonus for qualified minions
AssignedMinions = COUNT(Minions WHERE CurrentSchemeId = Scheme.SchemeId)
MatchingSpecialtyMinions = COUNT(Minions WHERE CurrentSchemeId = Scheme.SchemeId
                                  AND Specialty = Scheme.RequiredSpecialty)

MinionBonus = (MatchingSpecialtyMinions * 10)

// Bonus for assigned equipment
AssignedEquipment = COUNT(Equipment WHERE AssignedToSchemeId = Scheme.SchemeId
                          AND Condition >= 50)
EquipmentBonus = (AssignedEquipment * 5)

// Penalties
BudgetPenalty = (Scheme.CurrentSpending > Scheme.Budget) ? -20 : 0

RequiredMinionsMet = (AssignedMinions >= 2 AND MatchingSpecialtyMinions >= 1)
ResourcePenalty = RequiredMinionsMet ? 0 : -15

DeadlinePassed = (Today > Scheme.TargetCompletionDate)
TimelinePenalty = DeadlinePassed ? -25 : 0

// Final calculation
SuccessLikelihood = BaseSuccessLikelihood +
                   MinionBonus +
                   EquipmentBonus +
                   BudgetPenalty +
                   ResourcePenalty +
                   TimelinePenalty

// Clamp to valid range
SuccessLikelihood = MAX(0, MIN(100, SuccessLikelihood))
```

### Test Cases

| Scenario | Matching Minions | Equipment | Over Budget? | Deadline Passed? | Expected | Calculation |
|----------|-----------------|-----------|-------------|-----------------|----------|-------------|
| Bare minimum | 1 | 0 | No | No | 45 | 50 + 10 + 0 - 0 - 15 - 0 |
| Well resourced | 3 | 4 | No | No | 100 | 50 + 30 + 20 + 0 + 0 + 0 (clamped) |
| Over budget | 2 | 2 | Yes | No | 50 | 50 + 20 + 10 - 20 + 0 - 0 |
| Failed deadline | 2 | 2 | No | Yes | 35 | 50 + 20 + 10 + 0 + 0 - 25 |
| Complete failure | 0 | 0 | Yes | Yes | 0 | 50 + 0 + 0 - 20 - 15 - 25 (clamped) |

### Edge Cases

- Equipment with condition < 50 doesn't count toward bonus
- Minions with non-matching specialty don't provide the +10 bonus
- Multiple penalties stack (can drop to 0%)
- Multiple bonuses stack (can reach 100%)
- Calculation should update in real-time as resources change

### Implementation Note (Anti-Pattern)

This calculation is implemented in at least 3 places:
- SchemeManagementForm (on every UI update)
- MainForm (for dashboard statistics)
- Somewhere in Models or DatabaseHelper

Students should consolidate into a single, testable location.

---

## Rule 2: Budget Enforcement

**Rule ID**: BR-S-002
**Priority**: Critical
**Category**: Validation

### Description

Schemes cannot exceed their allocated budget. Current spending must be tracked and validated.

### Specification

```
IF (Scheme.CurrentSpending > Scheme.Budget) THEN
    Status = "Over Budget - Action Required"
    AllowNewAssignments = FALSE
    DisplayWarning("This scheme has exceeded its budget!")
ELSE IF (Scheme.CurrentSpending > Scheme.Budget * 0.9) THEN
    Status = "Approaching Budget Limit"
    AllowNewAssignments = TRUE
    DisplayWarning("Warning: Scheme is at 90% of budget")
ELSE
    Status = "Within Budget"
    AllowNewAssignments = TRUE
END IF
```

### Test Cases

| Budget | Current Spending | Status | Allow New Assignments? |
|--------|-----------------|--------|----------------------|
| 100000 | 50000 | Within Budget | Yes |
| 100000 | 91000 | Approaching Limit | Yes (with warning) |
| 100000 | 100001 | Over Budget | No |
| 100000 | 100000 | Within Budget | Yes (exactly at budget) |

### Spending Calculation

When a minion is assigned to a scheme:

```
EstimatedMonthsRemaining = MONTHS_BETWEEN(Today, Scheme.TargetCompletionDate)
IF (EstimatedMonthsRemaining < 1) THEN
    EstimatedMonthsRemaining = 1  // At least 1 month
END IF

AdditionalSpending = Minion.SalaryDemand * EstimatedMonthsRemaining
NewCurrentSpending = Scheme.CurrentSpending + AdditionalSpending

IF (NewCurrentSpending > Scheme.Budget) THEN
    Assignment = REJECTED("Would exceed scheme budget")
ELSE
    Scheme.CurrentSpending = NewCurrentSpending
    Assignment = ALLOWED
END IF
```

### Edge Cases

- Equipment assignment doesn't add to spending (assumes already purchased)
- Maintenance costs are separate (not tracked in scheme budget)
- If minion is unassigned, spending should decrease (but often doesn't - bug for students to find)
- Budget can be increased after scheme starts (recalculates spending projections)

---

## Rule 3: Status Transition Rules

**Rule ID**: BR-S-003
**Priority**: High
**Category**: Business Logic

### Description

Schemes transition through status states based on resource assignments and outcomes.

### Specification

**Valid Transitions**:

```
Planning → Active:
  - REQUIRES: At least 2 minions assigned
  - REQUIRES: At least 1 minion with matching specialty
  - REQUIRES: StartDate is set
  - REQUIRES: CurrentSpending <= Budget

Active → Completed:
  - REQUIRES: SuccessLikelihood >= 70%
  - REQUIRES: Today >= TargetCompletionDate
  - AUTOMATIC: Marks as completed

Active → Failed:
  - AUTOMATIC: If Today > TargetCompletionDate AND SuccessLikelihood < 30%
  - MANUAL: Can be manually failed at any time

Active → On Hold:
  - MANUAL: Can be paused at any time
  - PRESERVES: All resource assignments

On Hold → Active:
  - REQUIRES: Same conditions as Planning → Active
  - RESTORES: Previous state

Any Status → Planning:
  - MANUAL: Can restart scheme
  - CLEARS: All resource assignments
  - RESETS: CurrentSpending to 0
```

### State Diagram

```
    ┌──────────┐
    │ Planning │
    └────┬─────┘
         │ (resources assigned)
         ↓
    ┌────────┐      ┌─────────┐
    │ Active │←────→│ On Hold │
    └───┬─┬──┘      └─────────┘
        │ │
        │ │ (deadline + success check)
        │ ↓
        │ ┌───────────┐
        │ │ Completed │
        │ └───────────┘
        │
        │ (deadline + failure check)
        ↓
    ┌────────┐
    │ Failed │
    └────────┘
```

### Test Cases

| Current Status | Action | Minions | Success % | Result | Reason |
|---------------|--------|---------|-----------|--------|--------|
| Planning | Activate | 3 | 60 | Active | Requirements met |
| Planning | Activate | 1 | 50 | Rejected | Not enough minions |
| Active | Auto-check | 3 | 75 | Completed | Deadline reached, high success |
| Active | Auto-check | 3 | 25 | Failed | Deadline passed, low success |
| Active | Pause | 2 | 60 | On Hold | Manual pause |
| On Hold | Resume | 2 | 60 | Active | Restore to active |

### Edge Cases

- Cannot transition to Active without StartDate (but form might allow setting status without validation - bug)
- Auto-transitions should happen on form load or when viewing schemes
- Manual transitions via ComboBox should validate prerequisites
- Failed schemes cannot be reactivated (must reset to Planning)

---

## Rule 4: Resource Assignment Requirements

**Rule ID**: BR-S-004
**Priority**: High
**Category**: Validation

### Description

Different scheme types have different resource requirements based on their diabolical rating.

### Specification

```
IF (Scheme.DiabolicalRating >= 8) THEN
    RequiresDoomsdayDevice = TRUE
    RequiredEquipment = 2
    RequiredMinions = 3
ELSE IF (Scheme.DiabolicalRating >= 5) THEN
    RequiresDoomsdayDevice = FALSE
    RequiredEquipment = 1
    RequiredMinions = 2
ELSE
    RequiresDoomsdayDevice = FALSE
    RequiredEquipment = 0
    RequiredMinions = 1
END IF

// Validation
AssignedEquipment = COUNT(Equipment WHERE AssignedToSchemeId = Scheme.SchemeId)
AssignedMinions = COUNT(Minions WHERE CurrentSchemeId = Scheme.SchemeId)
HasDoomsdayDevice = EXISTS(Equipment WHERE AssignedToSchemeId = Scheme.SchemeId
                           AND Category = "Doomsday Device")

IF (DiabolicalRating >= 8 AND NOT HasDoomsdayDevice) THEN
    CanActivate = FALSE
    Warning = "Highly diabolical schemes require a doomsday device"
END IF

IF (AssignedEquipment < RequiredEquipment OR
    AssignedMinions < RequiredMinions) THEN
    CanActivate = FALSE
    Warning = "Insufficient resources for this scheme"
END IF
```

### Test Cases

| Diabolical Rating | Assigned Minions | Assigned Equipment | Has Doomsday Device | Can Activate? | Warning |
|------------------|-----------------|-------------------|-------------------|--------------|---------|
| 9 | 3 | 2 | Yes | Yes | - |
| 9 | 3 | 2 | No | No | "Requires doomsday device" |
| 7 | 2 | 1 | No | Yes | - |
| 7 | 1 | 1 | No | No | "Need at least 2 minions" |
| 3 | 1 | 0 | No | Yes | - |

### Business Context

**Diabolical Rating Scale**:
- 1-4: Minor schemes (pranks, small heists)
- 5-7: Major schemes (city-wide chaos, large thefts)
- 8-10: World-threatening schemes (require ultimate weapons)

---

## Rule 5: Deadline Management

**Rule ID**: BR-S-005
**Priority**: Medium
**Category**: Validation

### Description

Scheme deadlines affect success likelihood and automatic status transitions.

### Specification

```
DaysUntilDeadline = (Scheme.TargetCompletionDate - Today).Days

IF (DaysUntilDeadline < 0) THEN
    DeadlineStatus = "OVERDUE"
    ApplySuccessPenalty = -25%
    IF (Scheme.Status = "Active" AND SuccessLikelihood < 30) THEN
        AutoTransitionTo = "Failed"
    END IF
ELSE IF (DaysUntilDeadline <= 7) THEN
    DeadlineStatus = "URGENT - Due in " + DaysUntilDeadline + " days"
ELSE IF (DaysUntilDeadline <= 30) THEN
    DeadlineStatus = "Due soon"
ELSE
    DeadlineStatus = "On track"
END IF
```

### Test Cases

| Target Date | Today | Days Until | Status | Auto-Transition? |
|------------|-------|-----------|--------|-----------------|
| 2025-12-31 | 2025-12-01 | 30 | Due soon | No |
| 2025-12-10 | 2025-12-05 | 5 | URGENT | No |
| 2025-11-30 | 2025-12-05 | -5 | OVERDUE | Check success % |
| 2025-11-30 | 2025-12-05 | -5 (20% success) | OVERDUE | Yes → Failed |
| 2025-11-30 | 2025-12-05 | -5 (80% success) | OVERDUE | No (can still succeed) |

### Edge Cases

- Deadlines in the past when scheme created (should warn user)
- StartDate after TargetCompletionDate (invalid, should reject)
- Changing deadline while scheme is Active (recalculates spending projections)
- Completed schemes ignore deadline (success already achieved)

---

## Rule 6: Specialty Matching

**Rule ID**: BR-S-006
**Priority**: Medium
**Category**: Validation

### Description

At least one assigned minion must have the specialty required by the scheme.

### Specification

```
RequiredSpecialty = Scheme.RequiredSpecialty
AssignedMinions = LIST(Minions WHERE CurrentSchemeId = Scheme.SchemeId)

MatchingMinions = COUNT(AssignedMinions WHERE Specialty = RequiredSpecialty)

IF (MatchingMinions = 0 AND Scheme.Status IN ["Active", "Attempting to Activate"]) THEN
    Validation = FAILED("No minions with required specialty assigned")
ELSE IF (MatchingMinions = 1) THEN
    Validation = WARNING("Only one minion with required specialty - risky!")
ELSE
    Validation = PASSED
END IF
```

### Test Cases

| Required Specialty | Assigned Minions (Specialties) | Result | Message |
|-------------------|-------------------------------|--------|---------|
| Hacking | Hacking, Combat | Pass | - |
| Hacking | Combat, Disguise | Fail | "No minions with required specialty" |
| Explosives | Explosives | Pass (warning) | "Only one minion - risky!" |
| Combat | Combat, Combat, Hacking | Pass | Multiple matching minions |

### Impact on Success Likelihood

- Each minion with matching specialty: +10% to success likelihood
- Minions with non-matching specialty: +0% (but still count toward resource requirements)
- No matching specialty: -15% penalty (from Rule BR-S-001)

---

## Rule 7: Scheme Budget Validation

**Rule ID**: BR-S-007
**Priority**: High
**Category**: Validation

### Description

Scheme budget must be reasonable and cannot be negative or impossibly high.

### Specification

```
MinimumBudget = 10000  // Even simple schemes cost something
MaximumBudget = 10000000  // Realistic upper limit for educational context

IF (Budget < MinimumBudget) THEN
    Validation = FAILED("Budget too low - minimum is 10,000 evil dollars")
ELSE IF (Budget > MaximumBudget) THEN
    Validation = WARNING("Budget seems unrealistic - are you sure?")
    // But allow it
ELSE IF (Budget < EstimatedCost) THEN
    Validation = WARNING("Budget may be insufficient for resource requirements")
ELSE
    Validation = PASSED
END IF

EstimatedCost = (RequiredMinions * AverageMinionSalary * EstimatedMonths) +
                (RequiredEquipment * AverageMaintenanceCost * EstimatedMonths)
```

### Test Cases

| Budget | Estimated Cost | Result | Message |
|--------|---------------|--------|---------|
| 50000 | 30000 | Pass | - |
| 5000 | 30000 | Fail | "Budget too low" |
| 30000 | 45000 | Pass (warning) | "May be insufficient" |
| 15000000 | 50000 | Pass (warning) | "Seems unrealistic" |

### Edge Cases

- Budget can be increased but not decreased while scheme is Active
- If budget reduced below CurrentSpending, scheme becomes "Over Budget"
- Zero budget technically invalid but application might allow (bug for students)

---

## Integration Rules

### Cross-Entity Impacts

**When a scheme status changes to Active**:
1. Set StartDate to today (if not already set)
2. Recalculate SuccessLikelihood
3. Update all assigned minions' "days on scheme" counter
4. Lock resource assignments from other schemes

**When a scheme is completed or failed**:
1. Unassign all minions (free them for other schemes)
2. Unassign all equipment (return to storage)
3. Final SuccessLikelihood is frozen (historical record)
4. No further spending allowed

**When a minion or equipment is assigned**:
1. Update CurrentSpending (for minions)
2. Recalculate SuccessLikelihood
3. Check if scheme can transition to Active (if in Planning)

**When scheme is deleted**:
1. Unassign all minions (set CurrentSchemeId = NULL)
2. Unassign all equipment (set AssignedToSchemeId = NULL)
3. No spending refund (funds are spent)

---

## Testing Recommendations for Students

### Unit Tests to Write

1. **Success likelihood calculation** (Rule BR-S-001):
   - Test base case (no resources)
   - Test each bonus independently
   - Test each penalty independently
   - Test boundary conditions (0% and 100%)
   - Test combination scenarios

2. **Budget enforcement** (Rule BR-S-002):
   - Test spending within budget
   - Test spending exceeds budget
   - Test assignment rejection when over budget
   - Test spending calculations

3. **Status transitions** (Rule BR-S-003):
   - Test each valid transition
   - Test invalid transition attempts
   - Test prerequisite checking
   - Test automatic transitions

### Integration Tests to Write

1. **Resource requirements** (Rule BR-S-004):
   - Test activation with insufficient resources
   - Test doomsday device requirement for high diabolical ratings
   - Test minimum minion requirements

2. **Deadline management** (Rule BR-S-005):
   - Test overdue detection
   - Test automatic failure on low success + overdue
   - Test deadline warnings

### Refactoring Opportunities

Students should identify and fix:
- ✗ Success likelihood calculation duplicated in 3+ places
- ✗ Status transition logic in UI ComboBox event handler
- ✗ Budget validation inconsistent across forms
- ✗ Resource requirements checked too late (after assignment)
- ✗ Hardcoded thresholds (70% success, 30% failure) scattered throughout

---

**Note for Instructors**: The success likelihood calculation is the "crown jewel" of business logic that needs refactoring. It's complex enough to require real thought, but not so complex that it's overwhelming. Students should extract it into a testable service class.
