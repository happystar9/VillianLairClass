using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service for minion-related business logic
    /// Extracted from Minion model and UI event handlers
    /// </summary>
    public class MinionService
    {
        private readonly IMinionRepository _minionRepository;

        public MinionService(IMinionRepository minionRepository)
        {
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
        }

        public IEnumerable<Minion> GetAllMinions()
        {
            return _minionRepository.GetAll();
        }

        public Minion GetMinionById(int minionId)
        {
            return _minionRepository.GetById(minionId);
        }

        public void CreateMinion(Minion minion)
        {
            _minionRepository.Insert(minion);
        }

        public void UpdateMinion(Minion minion)
        {
            _minionRepository.Update(minion);
        }

        public void DeleteMinion(int minionId)
        {
            _minionRepository.Delete(minionId);
        }

        /// <summary>
        /// Updates minion mood based on loyalty score
        /// Extracted from Minion.UpdateMood()
        /// </summary>
        public void UpdateMinionMood(Minion minion)
        {
            if (minion == null) throw new ArgumentNullException(nameof(minion));

            // Business rules for mood calculation
            if (minion.LoyaltyScore > AppSettings.Instance.HighLoyaltyThreshold)
                minion.MoodStatus = AppSettings.Instance.MoodHappy;
            else if (minion.LoyaltyScore < AppSettings.Instance.LowLoyaltyThreshold)
                minion.MoodStatus = AppSettings.Instance.MoodBetrayal;
            else
                minion.MoodStatus = AppSettings.Instance.MoodGrumpy;

            minion.LastMoodUpdate = DateTime.Now;

            _minionRepository.Update(minion);
        }

        /// <summary>
        /// Updates minion loyalty based on salary payment
        /// Extracted from Minion.UpdateLoyalty()
        /// </summary>
        public void UpdateMinionLoyalty(Minion minion, decimal actualSalaryPaid)
        {
            if (minion == null) throw new ArgumentNullException(nameof(minion));

            if (actualSalaryPaid >= minion.SalaryDemand)
            {
                minion.LoyaltyScore += AppSettings.Instance.LoyaltyGrowthRate;
            }
            else
            {
                minion.LoyaltyScore -= AppSettings.Instance.LoyaltyDecayRate;
            }

            // Clamp to valid range
            if (minion.LoyaltyScore > 100) minion.LoyaltyScore = 100;
            if (minion.LoyaltyScore < 0) minion.LoyaltyScore = 0;

            // Update mood based on new loyalty
            UpdateMinionMood(minion);
        }

        /// <summary>
        /// Validates if a specialty is valid
        /// Extracted from Minion.IsValidSpecialty() and ValidationHelper
        /// </summary>
        public bool IsValidSpecialty(string specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty))
                return false;

            return AppSettings.Instance.ValidSpecialties.Contains(specialty);
        }

        /// <summary>
        /// Gets count of minions by mood status
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public Dictionary<string, int> GetMinionMoodCounts()
        {
            var minions = GetAllMinions().ToList();
            var counts = new Dictionary<string, int>
            {
                { "Happy", 0 },
                { "Grumpy", 0 },
                { "Betrayal", 0 }
            };

            foreach (var minion in minions)
            {
                if (minion.LoyaltyScore > AppSettings.Instance.HighLoyaltyThreshold)
                    counts["Happy"]++;
                else if (minion.LoyaltyScore < AppSettings.Instance.LowLoyaltyThreshold)
                    counts["Betrayal"]++;
                else
                    counts["Grumpy"]++;
            }

            return counts;
        }

        /// <summary>
        /// Gets minions with low loyalty (potential betrayers)
        /// Business logic extracted from MainForm alerts
        /// </summary>
        public IEnumerable<Minion> GetLowLoyaltyMinions()
        {
            return GetAllMinions().Where(m => m.LoyaltyScore < AppSettings.Instance.LowLoyaltyThreshold);
        }

        /// <summary>
        /// Calculates total monthly salary costs
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public decimal CalculateTotalSalaryCosts()
        {
            return GetAllMinions().Sum(m => m.SalaryDemand);
        }

        /// <summary>
        /// Gets minions by specialty
        /// </summary>
        public IEnumerable<Minion> GetMinionsBySpecialty(string specialty)
        {
            return GetAllMinions().Where(m => m.Specialty == specialty);
        }

        /// <summary>
        /// Gets minions assigned to a specific scheme
        /// </summary>
        public IEnumerable<Minion> GetMinionsForScheme(int schemeId)
        {
            return GetAllMinions().Where(m => m.CurrentSchemeId == schemeId);
        }

        /// <summary>
        /// Gets minions assigned to a specific base
        /// </summary>
        public IEnumerable<Minion> GetMinionsAtBase(int baseId)
        {
            return GetAllMinions().Where(m => m.CurrentBaseId == baseId);
        }
    }
}
