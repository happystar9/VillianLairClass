using System;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Equipment model with business logic
    /// </summary>
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Condition { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal MaintenanceCost { get; set; }
        public int? AssignedToSchemeId { get; set; }
        public int? StoredAtBaseId { get; set; }
        public bool RequiresSpecialist { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Category}, Condition: {Condition}%)";
        }
    }
}
