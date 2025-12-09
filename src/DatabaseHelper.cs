using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager
{
    /// <summary>
    /// Static facade over repository pattern - maintains backward compatibility
    /// Now delegates to proper repository pattern implementation underneath
    /// </summary>
    public static class DatabaseHelper
    {
        private static RepositoryFactory _repositoryFactory = null;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            string dbPath = AppSettings.Instance.DatabasePath;
            _repositoryFactory = new RepositoryFactory(dbPath);
            _isInitialized = true;
        }

        public static void CreateSchemaIfNotExists()
        {
            // Schema creation now handled by RepositoryFactory
            // This method kept for backward compatibility
        }

        public static void SeedInitialData()
        {
            // Check if data already exists
            var existingMinions = _repositoryFactory.Minions.GetAll();
            if (existingMinions.Any())
                return; // Data already exists

            // Seeding is intentionally simplified here - in production, use proper data seeding
            // This is kept as a stub for backward compatibility
        }

        // ===== MINION CRUD OPERATIONS =====

        public static List<Minion> GetAllMinions()
        {
            return new List<Minion>(_repositoryFactory.Minions.GetAll());
        }

        public static Minion GetMinionById(int minionId)
        {
            return _repositoryFactory.Minions.GetById(minionId);
        }

        public static void InsertMinion(Minion minion)
        {
            _repositoryFactory.Minions.Insert(minion);
        }

        public static void UpdateMinion(Minion minion)
        {
            _repositoryFactory.Minions.Update(minion);
        }

        public static void DeleteMinion(int minionId)
        {
            _repositoryFactory.Minions.Delete(minionId);
        }

        // ===== EVIL SCHEME CRUD OPERATIONS =====

        public static List<EvilScheme> GetAllSchemes()
        {
            return new List<EvilScheme>(_repositoryFactory.Schemes.GetAll());
        }

        public static EvilScheme GetSchemeById(int schemeId)
        {
            return _repositoryFactory.Schemes.GetById(schemeId);
        }

        public static void InsertScheme(EvilScheme scheme)
        {
            _repositoryFactory.Schemes.Insert(scheme);
        }

        public static void UpdateScheme(EvilScheme scheme)
        {
            _repositoryFactory.Schemes.Update(scheme);
        }

        public static void DeleteScheme(int schemeId)
        {
            _repositoryFactory.Schemes.Delete(schemeId);
        }

        // ===== SECRET BASE CRUD OPERATIONS =====

        public static List<SecretBase> GetAllBases()
        {
            return new List<SecretBase>(_repositoryFactory.Bases.GetAll());
        }

        public static SecretBase GetBaseById(int baseId)
        {
            return _repositoryFactory.Bases.GetById(baseId);
        }

        public static void InsertBase(SecretBase baseObj)
        {
            _repositoryFactory.Bases.Insert(baseObj);
        }

        public static void UpdateBase(SecretBase baseObj)
        {
            _repositoryFactory.Bases.Update(baseObj);
        }

        public static void DeleteBase(int baseId)
        {
            _repositoryFactory.Bases.Delete(baseId);
        }

        // ===== EQUIPMENT CRUD OPERATIONS =====

        public static List<Equipment> GetAllEquipment()
        {
            return new List<Equipment>(_repositoryFactory.Equipment.GetAll());
        }

        public static Equipment GetEquipmentById(int equipmentId)
        {
            return _repositoryFactory.Equipment.GetById(equipmentId);
        }

        public static void InsertEquipment(Equipment equipment)
        {
            _repositoryFactory.Equipment.Insert(equipment);
        }

        public static void UpdateEquipment(Equipment equipment)
        {
            _repositoryFactory.Equipment.Update(equipment);
        }

        public static void DeleteEquipment(int equipmentId)
        {
            _repositoryFactory.Equipment.Delete(equipmentId);
        }

        // ===== HELPER QUERIES =====

        public static int GetBaseOccupancy(int baseId)
        {
            return _repositoryFactory.Minions.GetMinionsByBase(baseId).Count();
        }

        public static int GetSchemeAssignedMinionsCount(int schemeId)
        {
            return _repositoryFactory.Minions.GetMinionsByScheme(schemeId).Count();
        }

        public static int GetSchemeAssignedEquipmentCount(int schemeId)
        {
            return _repositoryFactory.Equipment.GetEquipmentByScheme(schemeId).Count();
        }
    }
}
