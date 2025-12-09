using System;
using System.Linq;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Static validation helper with duplicated logic (anti-pattern)
    /// </summary>
    public static class ValidationHelper
    {
        // Specialty validation (duplicated in models and forms)
        public static bool IsValidSpecialty(string specialty)
        {
            // Hardcoded list instead of using ConfigManager (anti-pattern)
            return specialty == "Hacking" || specialty == "Explosives" ||
                   specialty == "Disguise" || specialty == "Combat" ||
                   specialty == "Engineering" || specialty == "Piloting";
        }

        // Category validation (duplicated in models and forms)
        public static bool IsValidCategory(string category)
        {
            // Another hardcoded list (anti-pattern)
            return category == "Weapon" || category == "Vehicle" ||
                   category == "Gadget" || category == "Doomsday Device";
        }

        // Skill level validation (duplicated in forms)
        public static bool IsValidSkillLevel(int skillLevel)
        {
            return skillLevel >= 1 && skillLevel <= 10;
        }

        // Loyalty validation (duplicated in forms)
        public static bool IsValidLoyalty(int loyalty)
        {
            return loyalty >= 0 && loyalty <= 100;
        }

        // Condition validation (duplicated in forms)
        public static bool IsValidCondition(int condition)
        {
            return condition >= 0 && condition <= 100;
        }

        // Diabolical rating validation
        public static bool IsValidDiabolicalRating(int rating)
        {
            return rating >= 1 && rating <= 10;
        }

        // Security level validation
        public static bool IsValidSecurityLevel(int level)
        {
            return level >= 1 && level <= 10;
        }
    }
}
