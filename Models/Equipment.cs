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

        // Business logic: condition degradation
        public void DegradeCondition()
        {
            if (AssignedToSchemeId.HasValue)
            {
                // Check if scheme is active
                var scheme = DatabaseHelper.GetSchemeById(AssignedToSchemeId.Value);
                if (scheme != null && scheme.Status == AppSettings.Instance.StatusActive)
                {
                    int monthsSinceMaintenance = 1; // Simplified - should calculate from LastMaintenanceDate
                    int degradation = monthsSinceMaintenance * AppSettings.Instance.ConditionDegradationRate;
                    Condition -= degradation;

                    if (Condition < 0) Condition = 0;

                    DatabaseHelper.UpdateEquipment(this);
                }
            }
        }

        // Perform maintenance
        public decimal PerformMaintenance()
        {
            decimal cost;
            if (Category == "Doomsday Device")
            {
                cost = PurchasePrice * AppSettings.Instance.DoomsdayMaintenanceCostPercentage;
            }
            else
            {
                cost = PurchasePrice * AppSettings.Instance.MaintenanceCostPercentage;
            }

            Condition = 100;
            LastMaintenanceDate = DateTime.Now;

            DatabaseHelper.UpdateEquipment(this);

            return cost;
        }

        // Check if operational
        public bool IsOperational()
        {
            return Condition >= AppSettings.Instance.MinEquipmentCondition;
        }

        public bool IsBroken()
        {
            return Condition < AppSettings.Instance.BrokenEquipmentCondition;
        }

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Category}, Condition: {Condition}%)";
        }
    }
}
