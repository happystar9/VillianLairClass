using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly DatabaseContext _context;

        public EquipmentRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Equipment> GetAll()
        {
            var equipment = new List<Equipment>();
            var query = @"SELECT EquipmentId, Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate 
                         FROM Equipment";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    equipment.Add(new Equipment
                    {
                        EquipmentId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = reader.GetString(2),
                        Condition = reader.GetInt32(3),
                        PurchasePrice = reader.GetDecimal(4),
                        MaintenanceCost = reader.GetDecimal(5),
                        AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                        StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                        RequiresSpecialist = reader.GetInt32(8) == 1,
                        LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                    });
                }
            }
            return equipment;
        }

        public Equipment GetById(int id)
        {
            var query = @"SELECT EquipmentId, Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate 
                         FROM Equipment WHERE EquipmentId = @id";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Equipment
                        {
                            EquipmentId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Category = reader.GetString(2),
                            Condition = reader.GetInt32(3),
                            PurchasePrice = reader.GetDecimal(4),
                            MaintenanceCost = reader.GetDecimal(5),
                            AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            RequiresSpecialist = reader.GetInt32(8) == 1,
                            LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                        };
                    }
                }
            }
            return null;
        }

        public void Insert(Equipment entity)
        {
            var query = @"INSERT INTO Equipment (Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate)
                         VALUES (@name, @category, @condition, @purchasePrice, @maintenanceCost, @schemeId, 
                         @baseId, @requiresSpecialist, @lastMaintenance)";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@category", entity.Category);
                cmd.Parameters.AddWithValue("@condition", entity.Condition);
                cmd.Parameters.AddWithValue("@purchasePrice", entity.PurchasePrice);
                cmd.Parameters.AddWithValue("@maintenanceCost", entity.MaintenanceCost);
                cmd.Parameters.AddWithValue("@schemeId", entity.AssignedToSchemeId.HasValue ? (object)entity.AssignedToSchemeId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@baseId", entity.StoredAtBaseId.HasValue ? (object)entity.StoredAtBaseId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@requiresSpecialist", entity.RequiresSpecialist ? 1 : 0);
                cmd.Parameters.AddWithValue("@lastMaintenance", entity.LastMaintenanceDate.HasValue ? entity.LastMaintenanceDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Equipment entity)
        {
            var query = @"UPDATE Equipment SET Name = @name, Category = @category, Condition = @condition, 
                         PurchasePrice = @purchasePrice, MaintenanceCost = @maintenanceCost, 
                         AssignedToSchemeId = @schemeId, StoredAtBaseId = @baseId, 
                         RequiresSpecialist = @requiresSpecialist, LastMaintenanceDate = @lastMaintenance
                         WHERE EquipmentId = @id";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", entity.EquipmentId);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@category", entity.Category);
                cmd.Parameters.AddWithValue("@condition", entity.Condition);
                cmd.Parameters.AddWithValue("@purchasePrice", entity.PurchasePrice);
                cmd.Parameters.AddWithValue("@maintenanceCost", entity.MaintenanceCost);
                cmd.Parameters.AddWithValue("@schemeId", entity.AssignedToSchemeId.HasValue ? (object)entity.AssignedToSchemeId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@baseId", entity.StoredAtBaseId.HasValue ? (object)entity.StoredAtBaseId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@requiresSpecialist", entity.RequiresSpecialist ? 1 : 0);
                cmd.Parameters.AddWithValue("@lastMaintenance", entity.LastMaintenanceDate.HasValue ? entity.LastMaintenanceDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            var query = "DELETE FROM Equipment WHERE EquipmentId = @id";
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<Equipment> GetEquipmentByScheme(int schemeId)
        {
            var equipment = new List<Equipment>();
            var query = @"SELECT EquipmentId, Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate 
                         FROM Equipment WHERE AssignedToSchemeId = @schemeId";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@schemeId", schemeId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipment.Add(new Equipment
                        {
                            EquipmentId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Category = reader.GetString(2),
                            Condition = reader.GetInt32(3),
                            PurchasePrice = reader.GetDecimal(4),
                            MaintenanceCost = reader.GetDecimal(5),
                            AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            RequiresSpecialist = reader.GetInt32(8) == 1,
                            LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return equipment;
        }

        public IEnumerable<Equipment> GetEquipmentByBase(int baseId)
        {
            var equipment = new List<Equipment>();
            var query = @"SELECT EquipmentId, Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate 
                         FROM Equipment WHERE StoredAtBaseId = @baseId";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@baseId", baseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipment.Add(new Equipment
                        {
                            EquipmentId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Category = reader.GetString(2),
                            Condition = reader.GetInt32(3),
                            PurchasePrice = reader.GetDecimal(4),
                            MaintenanceCost = reader.GetDecimal(5),
                            AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            RequiresSpecialist = reader.GetInt32(8) == 1,
                            LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return equipment;
        }

        public IEnumerable<Equipment> GetEquipmentByCategory(string category)
        {
            var equipment = new List<Equipment>();
            var query = @"SELECT EquipmentId, Name, Category, Condition, PurchasePrice, MaintenanceCost, 
                         AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate 
                         FROM Equipment WHERE Category = @category";
            
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@category", category);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipment.Add(new Equipment
                        {
                            EquipmentId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Category = reader.GetString(2),
                            Condition = reader.GetInt32(3),
                            PurchasePrice = reader.GetDecimal(4),
                            MaintenanceCost = reader.GetDecimal(5),
                            AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            RequiresSpecialist = reader.GetInt32(8) == 1,
                            LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return equipment;
        }
    }
}
