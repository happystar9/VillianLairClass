using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public class SecretBaseRepository : ISecretBaseRepository
    {
        private readonly DatabaseContext _context;

        public SecretBaseRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<SecretBase> GetAll()
        {
            var bases = new List<SecretBase>();
            var query = @"SELECT BaseId, Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, 
                         HasDoomsdayDevice, IsDiscovered, LastInspectionDate 
                         FROM SecretBases";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    bases.Add(new SecretBase
                    {
                        BaseId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Location = reader.GetString(2),
                        Capacity = reader.GetInt32(3),
                        SecurityLevel = reader.GetInt32(4),
                        MonthlyMaintenanceCost = reader.GetDecimal(5),
                        HasDoomsdayDevice = reader.GetInt32(6) == 1,
                        IsDiscovered = reader.GetInt32(7) == 1,
                        LastInspectionDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8))
                    });
                }
            }
            return bases;
        }

        public SecretBase GetById(int id)
        {
            var query = @"SELECT BaseId, Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, 
                         HasDoomsdayDevice, IsDiscovered, LastInspectionDate 
                         FROM SecretBases WHERE BaseId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new SecretBase
                        {
                            BaseId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Location = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            SecurityLevel = reader.GetInt32(4),
                            MonthlyMaintenanceCost = reader.GetDecimal(5),
                            HasDoomsdayDevice = reader.GetInt32(6) == 1,
                            IsDiscovered = reader.GetInt32(7) == 1,
                            LastInspectionDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8))
                        };
                    }
                }
            }
            return null;
        }

        public void Insert(SecretBase entity)
        {
            var query = @"INSERT INTO SecretBases (Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, 
                         HasDoomsdayDevice, IsDiscovered, LastInspectionDate)
                         VALUES (@name, @location, @capacity, @security, @maintenance, @hasDoomsday, @discovered, @lastInspection)";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@location", entity.Location);
                cmd.Parameters.AddWithValue("@capacity", entity.Capacity);
                cmd.Parameters.AddWithValue("@security", entity.SecurityLevel);
                cmd.Parameters.AddWithValue("@maintenance", entity.MonthlyMaintenanceCost);
                cmd.Parameters.AddWithValue("@hasDoomsday", entity.HasDoomsdayDevice ? 1 : 0);
                cmd.Parameters.AddWithValue("@discovered", entity.IsDiscovered ? 1 : 0);
                cmd.Parameters.AddWithValue("@lastInspection", entity.LastInspectionDate.HasValue ? entity.LastInspectionDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(SecretBase entity)
        {
            var query = @"UPDATE SecretBases SET Name = @name, Location = @location, Capacity = @capacity, 
                         SecurityLevel = @security, MonthlyMaintenanceCost = @maintenance, 
                         HasDoomsdayDevice = @hasDoomsday, IsDiscovered = @discovered, LastInspectionDate = @lastInspection
                         WHERE BaseId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", entity.BaseId);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@location", entity.Location);
                cmd.Parameters.AddWithValue("@capacity", entity.Capacity);
                cmd.Parameters.AddWithValue("@security", entity.SecurityLevel);
                cmd.Parameters.AddWithValue("@maintenance", entity.MonthlyMaintenanceCost);
                cmd.Parameters.AddWithValue("@hasDoomsday", entity.HasDoomsdayDevice ? 1 : 0);
                cmd.Parameters.AddWithValue("@discovered", entity.IsDiscovered ? 1 : 0);
                cmd.Parameters.AddWithValue("@lastInspection", entity.LastInspectionDate.HasValue ? entity.LastInspectionDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            var query = "DELETE FROM SecretBases WHERE BaseId = @id";
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<SecretBase> GetBasesByLocation(string location)
        {
            var bases = new List<SecretBase>();
            var query = @"SELECT BaseId, Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, 
                         HasDoomsdayDevice, IsDiscovered, LastInspectionDate 
                         FROM SecretBases WHERE Location = @location";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@location", location);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bases.Add(new SecretBase
                        {
                            BaseId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Location = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            SecurityLevel = reader.GetInt32(4),
                            MonthlyMaintenanceCost = reader.GetDecimal(5),
                            HasDoomsdayDevice = reader.GetInt32(6) == 1,
                            IsDiscovered = reader.GetInt32(7) == 1,
                            LastInspectionDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8))
                        });
                    }
                }
            }
            return bases;
        }
    }
}
