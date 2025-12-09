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

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Status}, {SuccessLikelihood}% success)";
        }
    }
}
