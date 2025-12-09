using System;
using System.IO;
using System.Text.Json;

namespace VillainLairManager
{
    public class AppSettings
    {
        public string DatabasePath { get; set; } = "villainlair.db";
        public decimal DefaultMinionSalary { get; set; } = 5000.0m;
        public string DefaultMinionName { get; set; } = "New Minion";
        public string DefaultSpecialty { get; set; } = "Unknown";
        public string DefaultMood { get; set; } = "Neutral";
        public int DefaultMinionLoyalty { get; set; } = 50;
        public int MinEquipmentCondition { get; set; } = 50;
        public int SuccessLikelihoodHighThreshold { get; set; } = 70;
        public int SuccessLikelihoodMediumThreshold { get; set; } = 40;
        public int SuccessLikelihoodLowThreshold { get; set; } = 30;
        public int BaseSuccessLikelihood { get; set; } = 50;
        public int LowLoyaltyThreshold { get; set; } = 40;
        public int HighLoyaltyThreshold { get; set; } = 70;
        public int LoyaltyDecayRate { get; set; } = 5;
        public int LoyaltyGrowthRate { get; set; } = 3;
        public int ConditionDegradationRate { get; set; } = 5;
        public decimal MaintenanceCostPercentage { get; set; } = 0.15m;
        public decimal DoomsdayMaintenanceCostPercentage { get; set; } = 0.30m;
        public int BrokenEquipmentCondition { get; set; } = 20;
        public string MoodHappy { get; set; } = "Happy";
        public string MoodGrumpy { get; set; } = "Grumpy";
        public string MoodBetrayal { get; set; } = "Plotting Betrayal";
        public string StatusActive { get; set; } = "Active";
        public string[] ValidSpecialties { get; set; } = new string[] { "Hacking", "Explosives", "Disguise", "Combat", "Engineering", "Piloting" };
        public string[] ValidCategories { get; set; } = new string[] { "Weapon", "Vehicle", "Gadget", "Doomsday Device" };

        private static AppSettings _instance;

        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        private static AppSettings Load()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppSettings();
                }
            }
            catch
            {
                // Fall back to defaults
            }
            return new AppSettings();
        }
    }
}
