using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service for equipment business logic
    /// Extracted from Equipment model
    /// </summary>
    public class EquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ISchemeRepository _schemeRepository;

        public EquipmentService(
            IEquipmentRepository equipmentRepository,
            ISchemeRepository schemeRepository)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
        }

        public IEnumerable<Equipment> GetAllEquipment()
        {
            return _equipmentRepository.GetAll();
        }

        public Equipment GetEquipmentById(int equipmentId)
        {
            return _equipmentRepository.GetById(equipmentId);
        }

        public void CreateEquipment(Equipment equipment)
        {
            _equipmentRepository.Insert(equipment);
        }

        public void UpdateEquipment(Equipment equipment)
        {
            _equipmentRepository.Update(equipment);
        }

        public void DeleteEquipment(int equipmentId)
        {
            _equipmentRepository.Delete(equipmentId);
        }

        /// <summary>
        /// Degrades equipment condition based on usage
        /// Extracted from Equipment.DegradeCondition()
        /// </summary>
        public void DegradeEquipmentCondition(Equipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));

            if (equipment.AssignedToSchemeId.HasValue)
            {
                // Check if scheme is active
                var scheme = _schemeRepository.GetById(equipment.AssignedToSchemeId.Value);
                if (scheme != null && scheme.Status == AppSettings.Instance.StatusActive)
                {
                    int monthsSinceMaintenance = 1; // Simplified - should calculate from LastMaintenanceDate
                    int degradation = monthsSinceMaintenance * AppSettings.Instance.ConditionDegradationRate;
                    equipment.Condition -= degradation;

                    if (equipment.Condition < 0) equipment.Condition = 0;

                    _equipmentRepository.Update(equipment);
                }
            }
        }

        /// <summary>
        /// Performs maintenance on equipment and returns the cost
        /// Extracted from Equipment.PerformMaintenance()
        /// </summary>
        public decimal PerformMaintenance(Equipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));

            decimal cost;
            if (equipment.Category == "Doomsday Device")
            {
                cost = equipment.PurchasePrice * AppSettings.Instance.DoomsdayMaintenanceCostPercentage;
            }
            else
            {
                cost = equipment.PurchasePrice * AppSettings.Instance.MaintenanceCostPercentage;
            }

            equipment.Condition = 100;
            equipment.LastMaintenanceDate = DateTime.Now;

            _equipmentRepository.Update(equipment);

            return cost;
        }

        /// <summary>
        /// Checks if equipment is operational
        /// Extracted from Equipment.IsOperational()
        /// </summary>
        public bool IsEquipmentOperational(Equipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));
            return equipment.Condition >= AppSettings.Instance.MinEquipmentCondition;
        }

        /// <summary>
        /// Checks if equipment is broken
        /// Extracted from Equipment.IsBroken()
        /// </summary>
        public bool IsEquipmentBroken(Equipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));
            return equipment.Condition < AppSettings.Instance.BrokenEquipmentCondition;
        }

        /// <summary>
        /// Gets broken equipment
        /// Business logic extracted from MainForm alerts
        /// </summary>
        public IEnumerable<Equipment> GetBrokenEquipment()
        {
            return GetAllEquipment().Where(e => IsEquipmentBroken(e));
        }

        /// <summary>
        /// Calculates total equipment maintenance costs
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public decimal CalculateTotalMaintenanceCosts()
        {
            return GetAllEquipment().Sum(e => e.MaintenanceCost);
        }

        /// <summary>
        /// Gets equipment by category
        /// </summary>
        public IEnumerable<Equipment> GetEquipmentByCategory(string category)
        {
            return GetAllEquipment().Where(e => e.Category == category);
        }

        /// <summary>
        /// Gets equipment assigned to a scheme
        /// </summary>
        public IEnumerable<Equipment> GetEquipmentForScheme(int schemeId)
        {
            return GetAllEquipment().Where(e => e.AssignedToSchemeId == schemeId);
        }

        /// <summary>
        /// Gets operational equipment for a scheme
        /// </summary>
        public IEnumerable<Equipment> GetOperationalEquipmentForScheme(int schemeId)
        {
            return GetEquipmentForScheme(schemeId).Where(e => IsEquipmentOperational(e));
        }

        /// <summary>
        /// Gets equipment stored at a base
        /// </summary>
        public IEnumerable<Equipment> GetEquipmentAtBase(int baseId)
        {
            return GetAllEquipment().Where(e => e.StoredAtBaseId == baseId);
        }

        /// <summary>
        /// Validates equipment category
        /// Extracted from ValidationHelper
        /// </summary>
        public bool IsValidCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return false;

            return AppSettings.Instance.ValidCategories.Contains(category);
        }

        /// <summary>
        /// Validates condition value
        /// </summary>
        public bool IsValidCondition(int condition)
        {
            return condition >= 0 && condition <= 100;
        }
    }
}
