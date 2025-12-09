using System;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Secret Base model
    /// </summary>
    public class SecretBase
    {
        public int BaseId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int SecurityLevel { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }
        public bool HasDoomsdayDevice { get; set; }
        public bool IsDiscovered { get; set; }
        public DateTime? LastInspectionDate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Location}, Capacity: {Capacity})";
        }
    }
}
