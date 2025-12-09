using System;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Minion model with business logic mixed in (anti-pattern)
    /// </summary>
    public class Minion
    {
        public int MinionId { get; set; }
        public string Name { get; set; }
        public int SkillLevel { get; set; }
        public string Specialty { get; set; }
        public int LoyaltyScore { get; set; }
        public decimal SalaryDemand { get; set; }
        public int? CurrentBaseId { get; set; }
        public int? CurrentSchemeId { get; set; }
        public string MoodStatus { get; set; }
        public DateTime LastMoodUpdate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Specialty}, Skill: {SkillLevel})";
        }
    }
}
