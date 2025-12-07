using System;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Evil Scheme model with business logic mixed in (anti-pattern)
    /// </summary>
    public class EvilScheme
    {
        public int SchemeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public decimal CurrentSpending { get; set; }
        public int RequiredSkillLevel { get; set; }
        public string RequiredSpecialty { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime TargetCompletionDate { get; set; }
        public int DiabolicalRating { get; set; }
        public int SuccessLikelihood { get; set; }

        // Business logic in model (anti-pattern)
        // This calculation is also duplicated in forms (major anti-pattern)
        public int CalculateSuccessLikelihood()
        {
            int baseSuccess = AppSettings.Instance.BaseSuccessLikelihood;

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
                    equipment.Condition >= AppSettings.Instance.MinEquipmentCondition)
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

        // Check if budget is exceeded
        public bool IsOverBudget()
        {
            return CurrentSpending > Budget;
        }

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Status}, {SuccessLikelihood}% success)";
        }
    }
}
