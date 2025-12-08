using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service for secret base business logic
    /// </summary>
    public class BaseService
    {
        private readonly ISecretBaseRepository _baseRepository;
        private readonly IMinionRepository _minionRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public BaseService(
            ISecretBaseRepository baseRepository,
            IMinionRepository minionRepository,
            IEquipmentRepository equipmentRepository)
        {
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        }

        public IEnumerable<SecretBase> GetAllBases()
        {
            return _baseRepository.GetAll();
        }

        public SecretBase GetBaseById(int baseId)
        {
            return _baseRepository.GetById(baseId);
        }

        public void CreateBase(SecretBase baseObj)
        {
            _baseRepository.Insert(baseObj);
        }

        public void UpdateBase(SecretBase baseObj)
        {
            _baseRepository.Update(baseObj);
        }

        public void DeleteBase(int baseId)
        {
            _baseRepository.Delete(baseId);
        }

        /// <summary>
        /// Calculates total monthly maintenance costs for all bases
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public decimal CalculateTotalMaintenanceCosts()
        {
            return GetAllBases().Sum(b => b.MonthlyMaintenanceCost);
        }

        /// <summary>
        /// Gets base occupancy (number of minions)
        /// </summary>
        public int GetBaseOccupancy(int baseId)
        {
            return _minionRepository.GetAll().Count(m => m.CurrentBaseId == baseId);
        }

        /// <summary>
        /// Checks if base is at capacity
        /// </summary>
        public bool IsBaseAtCapacity(int baseId)
        {
            var baseObj = GetBaseById(baseId);
            if (baseObj == null)
                return false;

            return GetBaseOccupancy(baseId) >= baseObj.Capacity;
        }

        /// <summary>
        /// Gets bases with doomsday devices
        /// </summary>
        public IEnumerable<SecretBase> GetBasesWithDoomsdayDevices()
        {
            return GetAllBases().Where(b => b.HasDoomsdayDevice);
        }

        /// <summary>
        /// Gets discovered bases
        /// </summary>
        public IEnumerable<SecretBase> GetDiscoveredBases()
        {
            return GetAllBases().Where(b => b.IsDiscovered);
        }

        /// <summary>
        /// Gets bases by location
        /// </summary>
        public IEnumerable<SecretBase> GetBasesByLocation(string location)
        {
            return GetAllBases().Where(b => b.Location == location);
        }

        /// <summary>
        /// Validates security level
        /// </summary>
        public bool IsValidSecurityLevel(int securityLevel)
        {
            return securityLevel >= 1 && securityLevel <= 10;
        }

        /// <summary>
        /// Validates capacity
        /// </summary>
        public bool IsValidCapacity(int capacity)
        {
            return capacity > 0;
        }
    }
}
