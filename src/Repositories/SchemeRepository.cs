using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public class SchemeRepository : ISchemeRepository
    {
        private readonly DatabaseContext _context;

        public SchemeRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<EvilScheme> GetAll()
        {
            var schemes = new List<EvilScheme>();
            var query = @"SELECT SchemeId, Name, Description, Budget, CurrentSpending, RequiredSkillLevel, 
                         RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood 
                         FROM EvilSchemes";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    schemes.Add(new EvilScheme
                    {
                        SchemeId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Budget = reader.GetDecimal(3),
                        CurrentSpending = reader.GetDecimal(4),
                        RequiredSkillLevel = reader.GetInt32(5),
                        RequiredSpecialty = reader.GetString(6),
                        Status = reader.GetString(7),
                        StartDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8)),
                        TargetCompletionDate = DateTime.Parse(reader.GetString(9)),
                        DiabolicalRating = reader.GetInt32(10),
                        SuccessLikelihood = reader.GetInt32(11)
                    });
                }
            }
            return schemes;
        }

        public EvilScheme GetById(int id)
        {
            var query = @"SELECT SchemeId, Name, Description, Budget, CurrentSpending, RequiredSkillLevel, 
                         RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood 
                         FROM EvilSchemes WHERE SchemeId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new EvilScheme
                        {
                            SchemeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Budget = reader.GetDecimal(3),
                            CurrentSpending = reader.GetDecimal(4),
                            RequiredSkillLevel = reader.GetInt32(5),
                            RequiredSpecialty = reader.GetString(6),
                            Status = reader.GetString(7),
                            StartDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8)),
                            TargetCompletionDate = DateTime.Parse(reader.GetString(9)),
                            DiabolicalRating = reader.GetInt32(10),
                            SuccessLikelihood = reader.GetInt32(11)
                        };
                    }
                }
            }
            return null;
        }

        public void Insert(EvilScheme entity)
        {
            var query = @"INSERT INTO EvilSchemes (Name, Description, Budget, CurrentSpending, RequiredSkillLevel, 
                         RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood)
                         VALUES (@name, @desc, @budget, @spending, @skillLevel, @specialty, @status, @startDate, 
                         @targetDate, @rating, @likelihood)";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@desc", entity.Description);
                cmd.Parameters.AddWithValue("@budget", entity.Budget);
                cmd.Parameters.AddWithValue("@spending", entity.CurrentSpending);
                cmd.Parameters.AddWithValue("@skillLevel", entity.RequiredSkillLevel);
                cmd.Parameters.AddWithValue("@specialty", entity.RequiredSpecialty);
                cmd.Parameters.AddWithValue("@status", entity.Status);
                cmd.Parameters.AddWithValue("@startDate", entity.StartDate.HasValue ? entity.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@targetDate", entity.TargetCompletionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@rating", entity.DiabolicalRating);
                cmd.Parameters.AddWithValue("@likelihood", entity.SuccessLikelihood);
                cmd.ExecuteNonQuery();

                // Retrieve the generated ID
                entity.SchemeId = (int)(long)_context.Connection.LastInsertRowId;
            }
        }

        public void Update(EvilScheme entity)
        {
            var query = @"UPDATE EvilSchemes SET Name = @name, Description = @desc, Budget = @budget, 
                         CurrentSpending = @spending, RequiredSkillLevel = @skillLevel, RequiredSpecialty = @specialty, 
                         Status = @status, StartDate = @startDate, TargetCompletionDate = @targetDate, 
                         DiabolicalRating = @rating, SuccessLikelihood = @likelihood
                         WHERE SchemeId = @id";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", entity.SchemeId);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@desc", entity.Description);
                cmd.Parameters.AddWithValue("@budget", entity.Budget);
                cmd.Parameters.AddWithValue("@spending", entity.CurrentSpending);
                cmd.Parameters.AddWithValue("@skillLevel", entity.RequiredSkillLevel);
                cmd.Parameters.AddWithValue("@specialty", entity.RequiredSpecialty);
                cmd.Parameters.AddWithValue("@status", entity.Status);
                cmd.Parameters.AddWithValue("@startDate", entity.StartDate.HasValue ? entity.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@targetDate", entity.TargetCompletionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@rating", entity.DiabolicalRating);
                cmd.Parameters.AddWithValue("@likelihood", entity.SuccessLikelihood);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            var query = "DELETE FROM EvilSchemes WHERE SchemeId = @id";
            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<EvilScheme> GetSchemesByStatus(string status)
        {
            var schemes = new List<EvilScheme>();
            var query = @"SELECT SchemeId, Name, Description, Budget, CurrentSpending, RequiredSkillLevel, 
                         RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood 
                         FROM EvilSchemes WHERE Status = @status";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@status", status);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schemes.Add(new EvilScheme
                        {
                            SchemeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Budget = reader.GetDecimal(3),
                            CurrentSpending = reader.GetDecimal(4),
                            RequiredSkillLevel = reader.GetInt32(5),
                            RequiredSpecialty = reader.GetString(6),
                            Status = reader.GetString(7),
                            StartDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8)),
                            TargetCompletionDate = DateTime.Parse(reader.GetString(9)),
                            DiabolicalRating = reader.GetInt32(10),
                            SuccessLikelihood = reader.GetInt32(11)
                        });
                    }
                }
            }
            return schemes;
        }

        public IEnumerable<EvilScheme> GetOverdueSchemes()
        {
            var schemes = new List<EvilScheme>();
            var query = @"SELECT SchemeId, Name, Description, Budget, CurrentSpending, RequiredSkillLevel, 
                         RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood 
                         FROM EvilSchemes WHERE TargetCompletionDate < @now AND Status != 'Completed' AND Status != 'Failed'";

            using (var cmd = new SQLiteCommand(query, _context.Connection))
            {
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schemes.Add(new EvilScheme
                        {
                            SchemeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Budget = reader.GetDecimal(3),
                            CurrentSpending = reader.GetDecimal(4),
                            RequiredSkillLevel = reader.GetInt32(5),
                            RequiredSpecialty = reader.GetString(6),
                            Status = reader.GetString(7),
                            StartDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8)),
                            TargetCompletionDate = DateTime.Parse(reader.GetString(9)),
                            DiabolicalRating = reader.GetInt32(10),
                            SuccessLikelihood = reader.GetInt32(11)
                        });
                    }
                }
            }
            return schemes;
        }
    }
}
