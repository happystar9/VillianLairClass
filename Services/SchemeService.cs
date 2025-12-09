using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service for evil scheme business logic
    /// Extracted from EvilScheme model and SchemeManagementForm
    /// </summary>
    public class SchemeService
    {
        private readonly ISchemeRepository _schemeRepository;
        private readonly IMinionRepository _minionRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public SchemeService(
            ISchemeRepository schemeRepository,
            IMinionRepository minionRepository,
            IEquipmentRepository equipmentRepository)
        {
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        }

        public IEnumerable<EvilScheme> GetAllSchemes()
        {
            return _schemeRepository.GetAll();
        }

        public EvilScheme GetSchemeById(int schemeId)
        {
            return _schemeRepository.GetById(schemeId);
        }

        public void CreateScheme(EvilScheme scheme)
        {
            _schemeRepository.Insert(scheme);
        }

        public void UpdateScheme(EvilScheme scheme)
        {
            _schemeRepository.Update(scheme);
        }

        public void DeleteScheme(int schemeId)
        {
            _schemeRepository.Delete(schemeId);
        }

        /// <summary>
        /// Calculates success likelihood for a scheme
        /// Extracted from EvilScheme.CalculateSuccessLikelihood() and SchemeManagementForm.CalculateSuccessLikelihoodInUI()
        /// This is the authoritative business logic - duplicates should be removed later
        /// </summary>
        public int CalculateSuccessLikelihood(EvilScheme scheme)
        {
            if (scheme == null) throw new ArgumentNullException(nameof(scheme));

            int baseSuccess = AppSettings.Instance.BaseSuccessLikelihood;

            // Get assigned minions from database
            var assignedMinions = _minionRepository.GetAll();
            int matchingMinions = 0;
            int totalMinions = 0;

            foreach (var minion in assignedMinions)
            {
                if (minion.CurrentSchemeId == scheme.SchemeId)
                {
                    totalMinions++;
                    if (minion.Specialty == scheme.RequiredSpecialty)
                    {
                        matchingMinions++;
                    }
                }
            }

            int minionBonus = matchingMinions * 10;

            // Get assigned equipment
            var assignedEquipment = _equipmentRepository.GetAll();
            int workingEquipmentCount = 0;

            foreach (var equipment in assignedEquipment)
            {
                if (equipment.AssignedToSchemeId == scheme.SchemeId &&
                    equipment.Condition >= AppSettings.Instance.MinEquipmentCondition)
                {
                    workingEquipmentCount++;
                }
            }

            int equipmentBonus = workingEquipmentCount * 5;

            // Penalties
            int budgetPenalty = (scheme.CurrentSpending > scheme.Budget) ? -20 : 0;
            int resourcePenalty = (totalMinions >= 2 && matchingMinions >= 1) ? 0 : -15;
            int timelinePenalty = (DateTime.Now > scheme.TargetCompletionDate) ? -25 : 0;

            // Calculate final
            int success = baseSuccess + minionBonus + equipmentBonus + budgetPenalty + resourcePenalty + timelinePenalty;

            // Clamp to 0-100
            if (success < 0) success = 0;
            if (success > 100) success = 100;

            return success;
        }

        /// <summary>
        /// Checks if scheme is over budget
        /// Extracted from EvilScheme.IsOverBudget()
        /// </summary>
        public bool IsSchemeOverBudget(EvilScheme scheme)
        {
            if (scheme == null) throw new ArgumentNullException(nameof(scheme));
            return scheme.CurrentSpending > scheme.Budget;
        }

        /// <summary>
        /// Gets schemes by status
        /// Business logic for filtering
        /// </summary>
        public IEnumerable<EvilScheme> GetSchemesByStatus(string status)
        {
            return GetAllSchemes().Where(s => s.Status == status);
        }

        /// <summary>
        /// Gets active schemes
        /// </summary>
        public IEnumerable<EvilScheme> GetActiveSchemes()
        {
            return GetSchemesByStatus("Active");
        }

        /// <summary>
        /// Gets schemes that are over budget
        /// Business logic extracted from MainForm alerts
        /// </summary>
        public IEnumerable<EvilScheme> GetOverBudgetSchemes()
        {
            return GetAllSchemes().Where(s => IsSchemeOverBudget(s));
        }

        /// <summary>
        /// Calculates average success likelihood for active schemes
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public double CalculateAverageSuccessLikelihood()
        {
            var activeSchemes = GetActiveSchemes().ToList();
            
            if (!activeSchemes.Any())
                return 0;

            double totalSuccess = 0;
            foreach (var scheme in activeSchemes)
            {
                totalSuccess += CalculateSuccessLikelihood(scheme);
            }

            return totalSuccess / activeSchemes.Count;
        }

        /// <summary>
        /// Gets overdue schemes (past target completion date and not completed/failed)
        /// </summary>
        public IEnumerable<EvilScheme> GetOverdueSchemes()
        {
            return GetAllSchemes().Where(s => 
                s.TargetCompletionDate < DateTime.Now && 
                s.Status != "Completed" && 
                s.Status != "Failed");
        }

        /// <summary>
        /// Validates required specialty
        /// </summary>
        public bool IsValidSpecialty(string specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty))
                return false;

            return AppSettings.Instance.ValidSpecialties.Contains(specialty);
        }

        /// <summary>
        /// Validates skill level
        /// </summary>
        public bool IsValidSkillLevel(int skillLevel)
        {
            return skillLevel >= 1 && skillLevel <= 10;
        }
    }
}
