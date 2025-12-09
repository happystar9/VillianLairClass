using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public class MinionRepository : IMinionRepository
    {
        private readonly DatabaseContext _context;

        public MinionRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Minion> GetAll()
        {
            var minions = new List<Minion>();
            var query = "SELECT MinionId, Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate FROM Minions";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    minions.Add(new Minion
                    {
                        MinionId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        SkillLevel = reader.GetInt32(2),
                        Specialty = reader.GetString(3),
                        LoyaltyScore = reader.GetInt32(4),
                        SalaryDemand = reader.GetDecimal(5),
                        CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                        CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                        MoodStatus = reader.GetString(8),
                        LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                    });
                }
            }
            return minions;
        }

        public Minion GetById(int id)
        {
            var query = "SELECT MinionId, Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate FROM Minions WHERE MinionId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Minion
                        {
                            MinionId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            SkillLevel = reader.GetInt32(2),
                            Specialty = reader.GetString(3),
                            LoyaltyScore = reader.GetInt32(4),
                            SalaryDemand = reader.GetDecimal(5),
                            CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            MoodStatus = reader.GetString(8),
                            LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                        };
                    }
                }
            }
            return null;
        }

        public void Insert(Minion entity)
        {
            var query = @"INSERT INTO Minions (Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate)
                         VALUES (@name, @skill, @specialty, @loyalty, @salary, @baseId, @schemeId, @mood, @lastMoodUpdate)";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@skill", entity.SkillLevel);
                cmd.Parameters.AddWithValue("@specialty", entity.Specialty);
                cmd.Parameters.AddWithValue("@loyalty", entity.LoyaltyScore);
                cmd.Parameters.AddWithValue("@salary", entity.SalaryDemand);
                cmd.Parameters.AddWithValue("@baseId", entity.CurrentBaseId.HasValue ? (object)entity.CurrentBaseId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@schemeId", entity.CurrentSchemeId.HasValue ? (object)entity.CurrentSchemeId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@mood", entity.MoodStatus);
                cmd.Parameters.AddWithValue("@lastMoodUpdate", entity.LastMoodUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();

                // Retrieve the generated ID
                entity.MinionId = (int)(long)_context.Connection.LastInsertRowId;
            }
        }

        public void Update(Minion entity)
        {
            var query = @"UPDATE Minions SET Name = @name, SkillLevel = @skill, Specialty = @specialty, 
                         LoyaltyScore = @loyalty, SalaryDemand = @salary, CurrentBaseId = @baseId, 
                         CurrentSchemeId = @schemeId, MoodStatus = @mood, LastMoodUpdate = @lastMoodUpdate
                         WHERE MinionId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", entity.MinionId);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@skill", entity.SkillLevel);
                cmd.Parameters.AddWithValue("@specialty", entity.Specialty);
                cmd.Parameters.AddWithValue("@loyalty", entity.LoyaltyScore);
                cmd.Parameters.AddWithValue("@salary", entity.SalaryDemand);
                cmd.Parameters.AddWithValue("@baseId", entity.CurrentBaseId.HasValue ? (object)entity.CurrentBaseId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@schemeId", entity.CurrentSchemeId.HasValue ? (object)entity.CurrentSchemeId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@mood", entity.MoodStatus);
                cmd.Parameters.AddWithValue("@lastMoodUpdate", entity.LastMoodUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            var query = "DELETE FROM Minions WHERE MinionId = @id";
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<Minion> GetMinionsByBase(int baseId)
        {
            var minions = new List<Minion>();
            var query = "SELECT MinionId, Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate FROM Minions WHERE CurrentBaseId = @baseId";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@baseId", baseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        minions.Add(new Minion
                        {
                            MinionId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            SkillLevel = reader.GetInt32(2),
                            Specialty = reader.GetString(3),
                            LoyaltyScore = reader.GetInt32(4),
                            SalaryDemand = reader.GetDecimal(5),
                            CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            MoodStatus = reader.GetString(8),
                            LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return minions;
        }

        public IEnumerable<Minion> GetMinionsByScheme(int schemeId)
        {
            var minions = new List<Minion>();
            var query = "SELECT MinionId, Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate FROM Minions WHERE CurrentSchemeId = @schemeId";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@schemeId", schemeId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        minions.Add(new Minion
                        {
                            MinionId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            SkillLevel = reader.GetInt32(2),
                            Specialty = reader.GetString(3),
                            LoyaltyScore = reader.GetInt32(4),
                            SalaryDemand = reader.GetDecimal(5),
                            CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            MoodStatus = reader.GetString(8),
                            LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return minions;
        }

        public IEnumerable<Minion> GetMinionsBySpecialty(string specialty)
        {
            var minions = new List<Minion>();
            var query = "SELECT MinionId, Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate FROM Minions WHERE Specialty = @specialty";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@specialty", specialty);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        minions.Add(new Minion
                        {
                            MinionId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            SkillLevel = reader.GetInt32(2),
                            Specialty = reader.GetString(3),
                            LoyaltyScore = reader.GetInt32(4),
                            SalaryDemand = reader.GetDecimal(5),
                            CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            MoodStatus = reader.GetString(8),
                            LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                        });
                    }
                }
            }
            return minions;
        }
    }
}
